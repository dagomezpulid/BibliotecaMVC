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

        /// <inheritdoc />
        public async Task<(bool Success, string Message)> ProcessLoanAsync(string userId, int libroId, int diasPrestamo)
        {
            try
            {
                var usuario = await _userManager.FindByIdAsync(userId);
                if (usuario == null) return (false, "Usuario no encontrado.");

                if (usuario.BloqueadoParaPrestamos)
                    return (false, "Tu cuenta está suspendida por morosidad.");

                var libro = await _context.Libros.Include(l => l.Archivos).FirstOrDefaultAsync(l => l.Id == libroId);
                if (libro == null) return (false, "El libro no existe.");

                if (!libro.Archivos.Any())
                    return (false, "El libro no tiene archivos digitales disponibles.");

                if (diasPrestamo < 2 || diasPrestamo > 20)
                    return (false, "Días de préstamo inválidos (debe ser entre 2 y 20).");

                var tieneMultaPendiente = await _context.Multas
                    .AnyAsync(m => m.Prestamo != null && m.Prestamo.UsuarioId == userId && !m.Pagada);

                if (tieneMultaPendiente)
                    return (false, "Tienes multas pendientes de pago.");

                var prestamosActivos = await _context.Prestamos.CountAsync(p => p.UsuarioId == userId && p.FechaDevolucionReal == null);
                if (prestamosActivos >= 3)
                    return (false, "Has alcanzado el límite de 3 préstamos activos.");

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

                // Notificaciones
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

        /// <inheritdoc />
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

                if (prestamo.DiasMora > 0)
                {
                    decimal totalMulta = prestamo.DiasMora * 1000;
                    var usuario = await _userManager.FindByIdAsync(userId);
                    
                    if (usuario != null)
                    {
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
