using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BibliotecaMVC.Services
{
    /// <summary>
    /// Implementación del servicio de préstamos.
    /// Encapsula la lógica de validación, multas y persistencia de préstamos.
    /// </summary>
    public class PrestamoService : IPrestamoService
    {
        private readonly BibliotecaContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;
        private readonly ILogger<PrestamoService> _logger;

        /// <summary>
        /// Inicializa el servicio con sus dependencias necesarias.
        /// </summary>
        public PrestamoService(
            BibliotecaContext context,
            UserManager<ApplicationUser> userManager,
            INotificationService notificationService,
            ILogger<PrestamoService> logger)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<List<Prestamo>> GetActiveLoansAsync(string userId)
        {
            return await _context.Prestamos
                .Include(p => p.Libro).ThenInclude(l => l!.Archivos)
                .Include(p => p.Usuario)
                .Where(p => p.UsuarioId == userId && p.FechaDevolucionReal == null)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<List<Prestamo>> GetLoanHistoryAsync(string userId)
        {
            return await _context.Prestamos
                .Include(p => p.Libro)
                .Where(p => p.UsuarioId == userId)
                .OrderByDescending(p => p.FechaPrestamo)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<List<Prestamo>> GetAllLoansAsync()
        {
            return await _context.Prestamos
                .Include(p => p.Libro).ThenInclude(l => l!.Archivos)
                .Include(p => p.Usuario)
                .OrderByDescending(p => p.FechaPrestamo)
                .ToListAsync();
        }

        /// <summary>
        /// Procesa la solicitud de un nuevo préstamo validando reglas de negocio estrictas.
        /// </summary>
        /// <param name="userId">ID del usuario solicitante.</param>
        /// <param name="libroId">ID del libro a prestar.</param>
        /// <param name="diasPrestamo">Duración solicitada (entre 2 y 20 días).</param>
        /// <returns>Tupla indicando éxito y un mensaje descriptivo.</returns>
        public async Task<(bool Success, string Message)> ProcessLoanAsync(string userId, int libroId, int diasPrestamo)
        {
            try
            {
                var usuario = await _userManager.FindByIdAsync(userId);
                if (usuario == null) return (false, "Usuario no encontrado.");

                // REGLA 1: No se permite prestar a usuarios con la cuenta suspendida/bloqueada
                if (usuario.BloqueadoParaPrestamos)
                    return (false, "Tu cuenta está suspendida por morosidad.");

                var libro = await _context.Libros.Include(l => l.Archivos).FirstOrDefaultAsync(l => l.Id == libroId);
                if (libro == null) return (false, "El libro no existe.");

                // REGLA 2: Solo se pueden prestar libros que tengan archivos digitales disponibles
                if (!libro.Archivos.Any())
                    return (false, "El libro no tiene archivos digitales disponibles.");

                if (diasPrestamo < 2 || diasPrestamo > 20)
                    return (false, "Días de préstamo inválidos (debe ser entre 2 y 20).");

                // REGLA 3: No se permite prestar si el usuario tiene deudas económicas (Multas sin pagar)
                var tieneMultaPendiente = await _context.Multas
                    .AnyAsync(m => m.Prestamo != null && m.Prestamo.UsuarioId == userId && !m.Pagada);

                if (tieneMultaPendiente)
                    return (false, "Tienes multas pendientes de pago.");

                // REGLA 4: Límite de capacidad de préstamo (Máximo 3 simultáneos)
                var prestamosActivos = await _context.Prestamos.CountAsync(p => p.UsuarioId == userId && p.FechaDevolucionReal == null);
                if (prestamosActivos >= 3)
                    return (false, "Has alcanzado el límite de 3 préstamos activos.");

                // REGLA 5: Evitar duplicidad de un mismo libro activo
                var yaLoTiene = await _context.Prestamos.AnyAsync(p => p.UsuarioId == userId && p.LibroId == libroId && p.FechaDevolucionReal == null);
                if (yaLoTiene)
                    return (false, "Ya tienes este libro en préstamo activo.");

                var prestamo = new Prestamo
                {
                    LibroId = libroId,
                    UsuarioId = userId,
                    FechaPrestamo = DateTime.Now,
                    FechaDevolucionProgramada = DateTime.Now.AddDays(diasPrestamo),
                    Estado = "Activo"
                };

                _context.Prestamos.Add(prestamo);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Préstamo exitoso: Usuario {UserId}, Libro {LibroId}", userId, libroId);

                // Notificaciones Multi-canal (SMS + In-App)
                await _notificationService.SendSmsAsync(usuario, libro.Titulo, $"Préstamo exitoso. Límite: {prestamo.FechaDevolucionProgramada.ToShortDateString()}.");
                await _notificationService.CreateNotificationAsync(userId, "📖 Préstamo Confirmado", $"Has alquilado '{libro.Titulo}'.", "success");

                return (true, "Préstamo realizado correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar préstamo para Usuario {UserId}, Libro {LibroId}", userId, libroId);
                return (false, "Ocurrió un error interno al procesar el préstamo.");
            }
        }

        /// <summary>
        /// Registra la devolución de un libro y calcula penalizaciones en caso de mora.
        /// </summary>
        /// <param name="userId">ID del usuario que devuelve.</param>
        /// <param name="prestamoId">ID del préstamo a cerrar.</param>
        /// <returns>Resultado de la operación con mensajes de advertencia si hubo multa.</returns>
        public async Task<(bool Success, string Message)> ProcessReturnAsync(string userId, int prestamoId)
        {
            try
            {
                var prestamo = await _context.Prestamos
                    .Include(p => p.Libro)
                    .FirstOrDefaultAsync(p => p.Id == prestamoId);

                if (prestamo == null) return (false, "Préstamo no encontrado.");
                if (prestamo.UsuarioId != userId) return (false, "No tienes permiso para devolver este préstamo.");
                if (prestamo.FechaDevolucionReal != null) return (false, "El préstamo ya fue devuelto anteriormente.");

                prestamo.FechaDevolucionReal = DateTime.Now;
                prestamo.Estado = "Devuelto";

                string message = "Libro devuelto correctamente.";

                // LÓGICA DE PENALIZACIÓN (MORA)
                if (prestamo.DiasMora > 0)
                {
                    // Tarifa estándar: $1000 por día de retraso
                    decimal totalMulta = prestamo.DiasMora * 1000;
                    var usuario = await _userManager.FindByIdAsync(userId);
                    
                    if (usuario != null)
                    {
                        // BLOQUEO DE SEGURIDAD: El usuario no puede volver a prestar hasta que pague
                        usuario.BloqueadoParaPrestamos = true;
                        await _userManager.UpdateAsync(usuario);

                        await _notificationService.SendSmsAsync(usuario, prestamo.Libro?.Titulo ?? "Libro", $"Devolución con retraso. Multa: ${totalMulta}. Cuenta bloqueada.");
                        await _notificationService.CreateNotificationAsync(userId, "⚠️ Multa Generada", $"Multa de ${totalMulta} por retraso.", "warning");
                        
                        message = "Has devuelto el libro con retraso. Tu cuenta ha sido SUSPENDIDA temporalmente.";
                    }

                    var multa = new Multa
                    {
                        PrestamoId = prestamo.Id,
                        Monto = totalMulta,
                        Pagada = false,
                        FechaGenerada = DateTime.Now
                    };
                    _context.Multas.Add(multa);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Devolución procesada: Préstamo {PrestamoId}, Mora: {DiasMora}", prestamoId, prestamo.DiasMora);

                return (true, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar devolución del préstamo {PrestamoId}", prestamoId);
                return (false, "Error interno al procesar la devolución.");
            }
        }

        /// <inheritdoc />
        public async Task<ProgresoLectura?> GetReadingProgressAsync(string userId, int libroId)
        {
            return await _context.ProgresosLectura
                .FirstOrDefaultAsync(p => p.UsuarioId == userId && p.LibroId == libroId);
        }

        /// <inheritdoc />
        public async Task SaveReadingProgressAsync(string userId, int libroId, int pagina)
        {
            try
            {
                var progreso = await _context.ProgresosLectura
                    .FirstOrDefaultAsync(p => p.UsuarioId == userId && p.LibroId == libroId);

                if (progreso == null)
                {
                    progreso = new ProgresoLectura
                    {
                        UsuarioId = userId,
                        LibroId = libroId,
                        PaginaActual = pagina,
                        UltimoAcceso = DateTime.Now
                    };
                    _context.ProgresosLectura.Add(progreso);
                }
                else
                {
                    progreso.PaginaActual = pagina;
                    progreso.UltimoAcceso = DateTime.Now;
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar progreso de lectura: Usuario {UserId}, Libro {LibroId}", userId, libroId);
            }
        }
    }
}
