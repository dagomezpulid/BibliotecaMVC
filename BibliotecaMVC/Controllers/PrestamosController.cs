using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using BibliotecaMVC.Services;

/// <summary>
/// Controlador principal para la gestión de préstamos.
/// Implementa reglas de negocio críticas como el límite de préstamos por usuario, 
/// prevención de IDOR, y notificaciones automáticas vía SMS (Twilio).
/// </summary>
[Authorize]
public class PrestamosController : Controller
{
    private readonly BibliotecaContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ISmsSender _smsSender;
    private readonly IWebHostEnvironment _env;

    /// <summary>
    /// Crea una notificación persistente en la base de datos para el usuario.
    /// </summary>
    private async Task CrearNotificacionAsync(string userId, string titulo, string contenido, string tipo = "info")
    {
        var notif = new Notificacion
        {
            UsuarioId = userId,
            Titulo = titulo,
            Contenido = contenido,
            Tipo = tipo,
            FechaCreacion = DateTime.Now,
            Leida = false
        };
        _context.Notificaciones.Add(notif);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Envía una notificación SMS al usuario de forma fire-and-forget.
    /// Solo se ejecuta si el usuario tiene número de teléfono registrado.
    /// </summary>
    /// <param name="usuario">Entidad del usuario receptor.</param>
    /// <param name="prestamo">Préstamo al que refiere el mensaje.</param>
    /// <param name="cuerpoPrincipal">Texto descriptivo del evento (ej: multa generada).</param>
    private void NotificarUsuarioSmsAsync(ApplicationUser usuario, Prestamo prestamo, string cuerpoPrincipal)
    {
        if (usuario != null && !string.IsNullOrEmpty(usuario.PhoneNumber))
        {
            string tituloLegible = prestamo.Libro?.Titulo ?? "solicitado";
            string smsBody = $"BibliotecaMVC: {cuerpoPrincipal} (Libro: '{tituloLegible}').";
            
            // Fire and forget descartable
            _ = _smsSender.SendSmsAsync(usuario.PhoneNumber, smsBody);
        }
    }

    /// <summary>
    /// Inicializa el controlador con todos los servicios requeridos.
    /// </summary>
    /// <param name="context">Contexto de datos de Entity Framework.</param>
    /// <param name="userManager">Gestor de identidades de ASP.NET Core Identity.</param>
    /// <param name="smsSender">Servicio de mensajería SMS (Twilio).</param>
    /// <param name="env">Entorno de ejecución para acceso al sistema de archivos.</param>
    public PrestamosController(
        BibliotecaContext context,
        UserManager<ApplicationUser> userManager,
        ISmsSender smsSender,
        IWebHostEnvironment env)
    {
        _context = context;
        _userManager = userManager;
        _smsSender = smsSender;
        _env = env;
    }

    /// <summary>
    /// Método auxiliar reutilizable: construye una consulta de préstamos con Libro, Usuario y Multa cargados.
    /// </summary>
    /// <returns>IQueryable preconfigurado con las relaciones necesarias.</returns>
    private IQueryable<Prestamo> ObtenerPrestamosDetallados()
    {
        return _context.Prestamos
            .Include(p => p.Libro)
            .Include(p => p.Usuario)
            .Include(p => p.Multa);
    }

    /// <summary>
    /// Lista los préstamos activos del usuario autenticado (sin fecha de devolución real).
    /// </summary>
    /// <returns>Vista con préstamos en curso del usuario actual.</returns>
    // Préstamos activos
    public IActionResult Index()
    {
        var usuarioId = _userManager.GetUserId(User);

        var prestamos = ObtenerPrestamosDetallados()
            .Where(p => p.UsuarioId == usuarioId && p.FechaDevolucionReal == null)
            .ToList();

        return View(prestamos);
    }

    /// <summary>
    /// Muestra el historial completo de préstamos (activos y devueltos) del usuario.
    /// Ordenados de más reciente a más antiguo.
    /// </summary>
    /// <returns>Vista con el historial de préstamos del usuario autenticado.</returns>
    // Historial
    public IActionResult Historial()
    {
        var usuarioId = _userManager.GetUserId(User);

        var historial = _context.Prestamos
            .Include(p => p.Libro)
            .Where(p => p.UsuarioId == usuarioId)
            .OrderByDescending(p => p.FechaPrestamo)
            .ToList();

        return View(historial);
    }

    /// <summary>
    /// Procesa la devolución de un libro.
    /// Ejecuta el peritaje de mora, genera multas si aplica, restaura el stock 
    /// y bloquea la cuenta del usuario si el entrega es tardía.
    /// </summary>
    /// <param name="id">ID del préstamo a devolver.</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Usuario")]
    public async Task<IActionResult> Devolver(int id)
    {
        var usuarioId = _userManager.GetUserId(User);

        var prestamo = await _context.Prestamos
            .Include(p => p.Libro)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (prestamo == null)
            return NotFound();

        // BUG: IDOR (Insecure Direct Object Reference). Impedir gestionar libros de otros.
        if (prestamo.UsuarioId != usuarioId)
            return Forbid();

        // BUG: Ataque de Doble Petición (Inflación del Stock)
        if (prestamo.FechaDevolucionReal != null)
        {
            TempData["Error"] = "Vulnerabilidad prevenida: El préstamo ya ha sido devuelto y el stock procesado.";
            return RedirectToAction(nameof(Index));
        }

        prestamo.FechaDevolucionReal = DateTime.Now;
        prestamo.Estado = "Devuelto";

        // Generar multa si aplica a nivel modelo
        if (prestamo.DiasMora > 0)
        {
            // CASTIGO AUTOMÁTICO: Bloquear cuenta del usuario local
            var usuarioInfractor = await _userManager.FindByIdAsync(usuarioId);
            if (usuarioInfractor != null)
            {
                usuarioInfractor.BloqueadoParaPrestamos = true;
                await _userManager.UpdateAsync(usuarioInfractor);
                TempData["Error"] += "Has devuelto el libro con retraso. Tu cuenta ha sido SUSPENDIDA temporalmente.";
            }

            decimal valorPorDia = 1000;
            decimal totalMulta = prestamo.DiasMora * valorPorDia;

            var multa = new Multa
            {
                PrestamoId = prestamo.Id,
                Monto = totalMulta,
                Pagada = false,
                FechaGenerada = DateTime.Now
            };

            _context.Multas.Add(multa);

            // 🚀 INTEGRACIÓN MUNDIAL TWILIO: Disparar el castigo al dispositivo celular
            NotificarUsuarioSmsAsync(usuarioInfractor, prestamo, $"Tu préstamo fue devuelto tarde. Se generó una multa de ${totalMulta}. Tu cuenta ha sido bloqueada hasta sanear tu deuda");

            // 🔔 Notificación Interna
            await CrearNotificacionAsync(usuarioId, "⚠️ Multa Generada", $"Se ha generado una multa de ${totalMulta} por retraso en el libro '{prestamo.Libro.Titulo}'.", "warning");
        }

        prestamo.Libro.Stock++;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Muestra la vista de confirmación del préstamo con todos los detalles del libro seleccionado.
    /// </summary>
    /// <param name="id">ID del libro a rentar.</param>
    [Authorize(Roles = "Usuario")]
    public async Task<IActionResult> ConfirmarPrestamo(int id)
    {
        var libro = await _context.Libros
            .Include(l => l.Autor)
            .Include(l => l.Categorias)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (libro == null)
            return NotFound();

        ViewBag.LibroTitulo = libro.Titulo;

        return View("Prestar", libro);
    }

    /// <summary>
    /// Crea un nuevo registro de préstamo en la base de datos.
    /// Valida: Suspensión de cuenta, Stock disponible, Rango de días (2-20),
    /// Deudas pendientes y Límite de 3 préstamos activos.
    /// </summary>
    /// <param name="libroId">ID del libro a rentar.</param>
    /// <param name="diasPrestamo">Duración elegida por el usuario.</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Usuario")]
    public async Task<IActionResult> Prestar(int libroId, int diasPrestamo)
    {
        var usuarioId = _userManager.GetUserId(User);

        // Candado Fuerte: Bloqueo Administrativo
        var usuarioDB = await _userManager.FindByIdAsync(usuarioId);
        if (usuarioDB != null && usuarioDB.BloqueadoParaPrestamos)
        {
            TempData["Error"] = "¡Cuenta Suspendida! Tuviste un retraso anterior. Un administrador debe evaluar tu caso para restablecer tu acceso.";
            return RedirectToAction("Index", "Libros");
        }

        var libro = await _context.Libros.FindAsync(libroId);

        if (libro == null || libro.Stock <= 0)
        {
            TempData["Error"] = "Lo sentimos, el libro solicitado ya no tiene ejemplares disponibles.";
            return RedirectToAction("Index", "Libros");
        }

        if (diasPrestamo < 2 || diasPrestamo > 20)
        {
            TempData["Error"] = "Cantidad de días inválida. La biblioteca solo permite préstamos entre 2 y 20 días.";
            return RedirectToAction("ConfirmarPrestamo", new { id = libroId });
        }

        // Validar multas pendientes
        var tieneMultaPendiente = await _context.Multas
            .Include(m => m.Prestamo)
            .AnyAsync(m =>
                m.Prestamo.UsuarioId == usuarioId &&
                !m.Pagada);

        if (tieneMultaPendiente)
        {
            TempData["Error"] =
                "No puedes realizar préstamos mientras tengas multas pendientes.";
            return RedirectToAction("Index", "Libros");
        }

        var prestamo = new Prestamo
        {
            LibroId = libroId,
            UsuarioId = usuarioId,
            FechaPrestamo = DateTime.Now,
            FechaDevolucionProgramada = DateTime.Now.AddDays(diasPrestamo),
            Estado = "Activo"
        };

        var prestamosActivos = await _context.Prestamos
            .CountAsync(p => p.UsuarioId == usuarioId && p.FechaDevolucionReal == null);

        if (prestamosActivos >= 3)
        {
            TempData["Error"] = "Solo puedes tener máximo 3 préstamos activos.";
            return RedirectToAction("Index", "Home");
        }

        var yaTieneLibro = await _context.Prestamos
            .AnyAsync(p =>
                p.UsuarioId == usuarioId &&
                p.LibroId == libroId &&
                p.FechaDevolucionReal == null);

        if (yaTieneLibro)
        {
            TempData["Error"] = "Ya tienes este libro prestado.";
            return RedirectToAction("Index", "Home");
        }

        try 
        {
            libro.Stock--;
            _context.Prestamos.Add(prestamo);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            TempData["Error"] = "Hubo un conflicto al procesar el stock. Otro usuario pudo haber rentado el último ejemplar simultáneamente. Por favor, intenta de nuevo.";
            return RedirectToAction("Index", "Libros");
        }

        // 🔥 Confirmación Inmediata al Usuario (SMS Vía Twilio)
        string fechaLim = prestamo.FechaDevolucionProgramada.ToShortDateString();
        NotificarUsuarioSmsAsync(usuarioDB, prestamo, $"Préstamo exitoso. Límite de devolución: {fechaLim}. Recuerda: Entregar tarde generará multas inmediatas y congelamiento de tu cuenta");

        // 🔔 Notificación Interna
        await CrearNotificacionAsync(usuarioId, "📖 Préstamo Confirmado", $"Has alquilado '{libro.Titulo}'. Fecha límite: {fechaLim}.", "success");

        TempData["Success"] = "Préstamo realizado correctamente.";

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Vista administrativa: Retorna todos los préstamos del sistema sin importar el usuario,
    /// ordenados por fecha de creación descendente.
    /// </summary>
    /// <returns>Vista con la lista global de préstamos para supervisión del admin.</returns>
    // Vista admin
    [Authorize(Roles = "Admin")]
    public IActionResult Todos()
    {
        var prestamos = ObtenerPrestamosDetallados()
            .OrderByDescending(p => p.FechaPrestamo)
            .ToList();

        return View(prestamos);
    }

    /// <summary>
    /// Motor de Consumo Web: Provee acceso al visor integrado para leer el libro.
    /// Valida estrictamente que el usuario tenga un préstamo activo.
    /// </summary>
    [Authorize(Roles = "Usuario")]
    public async Task<IActionResult> Leer(int id)
    {
        var usuarioId = _userManager.GetUserId(User);
        
        var prestamo = await _context.Prestamos
            .Include(p => p.Libro)
            .FirstOrDefaultAsync(p => p.Id == id && p.UsuarioId == usuarioId);

        if (prestamo == null) return NotFound();

        // DRM: Solo si el préstamo sigue activo
        if (prestamo.FechaDevolucionReal != null || prestamo.Estado != "Activo")
        {
            TempData["Error"] = "Tu préstamo ha expirado o ya fue devuelto. No puedes leer este libro.";
            return RedirectToAction(nameof(Index));
        }

        if (string.IsNullOrEmpty(prestamo.Libro.ArchivoRuta))
        {
            TempData["Error"] = "Lamentablemente este libro aún no cuenta con una versión digital subida a la plataforma.";
            return RedirectToAction(nameof(Index));
        }

        return View("Leer", prestamo.Libro);
    }

    /// <summary>
    /// Forzar la descarga del archivo original para formatos que no pueden leerse en web.
    /// Mismos candados de seguridad.
    /// </summary>
    [Authorize(Roles = "Usuario")]
    public async Task<IActionResult> DescargarLibro(int id)
    {
        var usuarioId = _userManager.GetUserId(User);
        var prestamo = await _context.Prestamos
            .Include(p => p.Libro)
            .FirstOrDefaultAsync(p => p.Id == id && p.UsuarioId == usuarioId && p.Estado == "Activo" && p.FechaDevolucionReal == null);

        if (prestamo == null || string.IsNullOrEmpty(prestamo.Libro?.ArchivoRuta))
        {
            return Forbid();
        }

        var vaultFolder = Path.Combine(_env.ContentRootPath, "BibliotecaLibros_Vault");
        var filePath = Path.Combine(vaultFolder, prestamo.Libro.ArchivoRuta);
        
        // Fallback for legacy files
        if (!System.IO.File.Exists(filePath) && prestamo.Libro.ArchivoRuta.StartsWith("/archivos_libros/"))
        {
            filePath = Path.Combine(_env.WebRootPath, prestamo.Libro.ArchivoRuta.TrimStart('/'));
        }

        if (!System.IO.File.Exists(filePath))
        {
            TempData["Error"] = "El archivo ya no existe en el servidor.";
            return RedirectToAction(nameof(Index));
        }

        var fileName = Path.GetFileName(filePath);
        var contentType = "application/octet-stream";
        
        return PhysicalFile(filePath, contentType, fileName);
    }

    /// <summary>
    /// Túnel DRM: Sirve el archivo al visor web únicamente si el usuario está autorizado.
    /// Evita fugas de URLs públicas desde el IFrame.
    /// </summary>
    [Authorize(Roles = "Usuario")]
    public async Task<IActionResult> ArchivoLectura(int id)
    {
        var usuarioId = _userManager.GetUserId(User);
        var prestamo = await _context.Prestamos
            .Include(p => p.Libro)
            .FirstOrDefaultAsync(p => p.Id == id && p.UsuarioId == usuarioId && p.Estado == "Activo" && p.FechaDevolucionReal == null);

        if (prestamo == null || string.IsNullOrEmpty(prestamo.Libro?.ArchivoRuta))
        {
            return Forbid();
        }

        var vaultFolder = Path.Combine(_env.ContentRootPath, "BibliotecaLibros_Vault");
        var filePath = Path.Combine(vaultFolder, prestamo.Libro.ArchivoRuta);
        
        // Fallback for legacy files
        if (!System.IO.File.Exists(filePath) && prestamo.Libro.ArchivoRuta.StartsWith("/archivos_libros/"))
        {
            filePath = Path.Combine(_env.WebRootPath, prestamo.Libro.ArchivoRuta.TrimStart('/'));
        }

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        // Determinar ContentType
        string contentType = "application/octet-stream";
        if (filePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)) contentType = "application/pdf";
        else if (filePath.EndsWith(".epub", StringComparison.OrdinalIgnoreCase)) contentType = "application/epub+zip";
        else if (filePath.EndsWith(".docx", StringComparison.OrdinalIgnoreCase)) contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
        else if (filePath.EndsWith(".doc", StringComparison.OrdinalIgnoreCase)) contentType = "application/msword";

        // Retorna un stream del archivo sin descargarlo (inline)
        return PhysicalFile(filePath, contentType, enableRangeProcessing: true);
    }
}


