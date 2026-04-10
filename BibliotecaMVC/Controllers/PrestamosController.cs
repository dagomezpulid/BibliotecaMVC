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

    public PrestamosController(
        BibliotecaContext context,
        UserManager<ApplicationUser> userManager,
        ISmsSender smsSender)
    {
        _context = context;
        _userManager = userManager;
        _smsSender = smsSender;
    }

    private IQueryable<Prestamo> ObtenerPrestamosDetallados()
    {
        return _context.Prestamos
            .Include(p => p.Libro)
            .Include(p => p.Usuario)
            .Include(p => p.Multa);
    }

    // Préstamos activos
    public IActionResult Index()
    {
        var usuarioId = _userManager.GetUserId(User);

        var prestamos = ObtenerPrestamosDetallados()
            .Where(p => p.UsuarioId == usuarioId && p.FechaDevolucionReal == null)
            .ToList();

        return View(prestamos);
    }

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

    [Authorize(Roles = "Usuario")]
    public async Task<IActionResult> ConfirmarPrestamo(int id)
    {
        var libro = await _context.Libros.FindAsync(id);

        if (libro == null)
            return NotFound();

        ViewBag.LibroTitulo = libro.Titulo;

        return View("Prestar", libro.Id);
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
            return BadRequest("Libro no disponible");

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

        libro.Stock--;

        _context.Prestamos.Add(prestamo);
        await _context.SaveChangesAsync();

        // 🔥 Confirmación Inmediata al Usuario (SMS Vía Twilio)
        string fechaLim = prestamo.FechaDevolucionProgramada.ToShortDateString();
        NotificarUsuarioSmsAsync(usuarioDB, prestamo, $"Préstamo exitoso. Límite de devolución: {fechaLim}. Recuerda: Entregar tarde generará multas inmediatas y congelamiento de tu cuenta");

        // 🔔 Notificación Interna
        await CrearNotificacionAsync(usuarioId, "📖 Préstamo Confirmado", $"Has alquilado '{libro.Titulo}'. Fecha límite: {fechaLim}.", "success");

        TempData["Success"] = "Préstamo realizado correctamente.";

        return RedirectToAction(nameof(Index));
    }

    // Vista admin
    [Authorize(Roles = "Admin")]
    public IActionResult Todos()
    {
        var prestamos = ObtenerPrestamosDetallados()
            .OrderByDescending(p => p.FechaPrestamo)
            .ToList();

        return View(prestamos);
    }
}


