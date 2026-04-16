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
    private void NotificarUsuarioSmsAsync(ApplicationUser usuario, Prestamo prestamo, string cuerpoPrincipal)
    {
        if (usuario != null && !string.IsNullOrEmpty(usuario.PhoneNumber))
        {
            string tituloLegible = prestamo.Libro?.Titulo ?? "solicitado";
            string smsBody = $"BibliotecaMVC: {cuerpoPrincipal} (Libro: '{tituloLegible}').";
            _ = _smsSender.SendSmsAsync(usuario.PhoneNumber, smsBody);
        }
    }

    /// <summary>
    /// Registra una acción de auditoría en la base de datos de forma asíncrona.
    /// </summary>
    private async Task RegistrarAuditoriaAsync(string accion, string? recursoId, string? detalles = null)
    {
        try
        {
            var log = new LogAuditoria
            {
                UsuarioId = _userManager.GetUserId(User) ?? "Anónimo",
                Accion = accion,
                RecursoId = recursoId,
                Detalles = detalles,
                Fecha = DateTime.Now,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };
            _context.LogsAuditoria.Add(log);
            await _context.SaveChangesAsync();
        }
        catch (Exception)
        {
            // Fallo silencioso en auditoría para no bloquear la experiencia de usuario
        }
    }

    /// <summary>
    /// Inicializa el controlador con los servicios de contexto, seguridad y mensajería.
    /// </summary>
    /// <param name="context">Contexto EF Core.</param>
    /// <param name="userManager">Gestor de usuarios para validación de roles y estados.</param>
    /// <param name="smsSender">Servicio de despacho de mensajes vía Twilio.</param>
    /// <param name="env">Hosting Environment para acceso a recursos protegidos.</param>
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

    private IQueryable<Prestamo> ObtenerPrestamosDetallados()
    {
        return _context.Prestamos
            .Include(p => p.Libro)
                .ThenInclude(l => l!.Archivos)
            .Include(p => p.Usuario)
            .Include(p => p.Multa);
    }

    /// <summary>
    /// Muestra los préstamos activos del usuario autenticado.
    /// </summary>
    public IActionResult Index()
    {
        var usuarioId = _userManager.GetUserId(User);
        var prestamos = ObtenerPrestamosDetallados()
            .Where(p => p.UsuarioId == usuarioId && p.FechaDevolucionReal == null)
            .ToList();
        return View(prestamos);
    }

    /// <summary>
    /// Muestra el histórico completo de préstamos (devueltos y activos) del usuario.
    /// </summary>
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
    /// Procesa la devolución de un libro digital.
    /// Si hay mora, bloquea al usuario y genera una multa financiera.
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

        if (prestamo == null) return NotFound();
        if (prestamo.UsuarioId != usuarioId) return Forbid();

        if (prestamo.FechaDevolucionReal != null)
        {
            TempData["Error"] = "El préstamo ya ha sido devuelto.";
            return RedirectToAction(nameof(Index));
        }

        prestamo.FechaDevolucionReal = DateTime.Now;
        prestamo.Estado = "Devuelto";

        if (prestamo.DiasMora > 0)
        {
            var usuarioInfractor = await _userManager.FindByIdAsync(usuarioId ?? "");
            decimal totalMulta = prestamo.DiasMora * 1000;

            if (usuarioInfractor != null)
            {
                usuarioInfractor.BloqueadoParaPrestamos = true;
                await _userManager.UpdateAsync(usuarioInfractor);
                
                NotificarUsuarioSmsAsync(usuarioInfractor, prestamo, $"Tu préstamo fue devuelto tarde. Multa: ${totalMulta}. Cuenta bloqueada.");
                await CrearNotificacionAsync(usuarioInfractor.Id, "⚠️ Multa Generada", $"Se ha generado una multa de ${totalMulta} por retraso.", "warning");
                
                TempData["Error"] = "Has devuelto el libro con retraso. Tu cuenta ha sido SUSPENDIDA temporalmente.";
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

        // Ya no existe préstamo.Libro.Stock++
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Paso previo al préstamo: Muestra la pasarela de selección de días.
    /// Valida que el libro cuente con copias digitales cargadas.
    /// </summary>
    /// <param name="id">ID del libro a solicitar.</param>
    [Authorize(Roles = "Usuario")]
    public async Task<IActionResult> ConfirmarPrestamo(int id)
    {
        var libro = await _context.Libros
            .Include(l => l.Autor)
            .Include(l => l.Categorias)
            .Include(l => l.Archivos)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (libro == null) return NotFound();

        // Si el libro no tiene archivos, informamos al usuario
        if (!libro.Archivos.Any())
        {
            TempData["Error"] = "Este libro aún no tiene una versión digital disponible para lectura.";
            return RedirectToAction("Index", "Libros");
        }

        ViewBag.LibroTitulo = libro.Titulo;
        return View("Prestar", libro);
    }

    /// <summary>
    /// Transacción de solicitud de préstamo digital.
    /// Valida: cuota de préstamos, morosidad previa, disponibilidad y límites de tiempo.
    /// </summary>
    /// <param name="libroId">ID del libro solicitado.</param>
    /// <param name="diasPrestamo">Rango de días seleccionado (2-20).</param>
    /// <returns>Redirección al índice de préstamos en éxito o al catálogo con error detallado.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Usuario")]
    public async Task<IActionResult> Prestar(int libroId, int diasPrestamo)
    {
        var usuarioId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(usuarioId)) return Unauthorized();

        var usuarioDB = await _userManager.FindByIdAsync(usuarioId);
        
        if (usuarioDB != null && usuarioDB.BloqueadoParaPrestamos)
        {
            TempData["Error"] = "Cuenta Suspendida por morosidad.";
            return RedirectToAction("Index", "Libros");
        }

        var libro = await _context.Libros.Include(l => l.Archivos).FirstOrDefaultAsync(l => l.Id == libroId);
        if (libro == null) return NotFound();

        // Eliminado chequeo de Stock físico
        if (!libro.Archivos.Any())
        {
            TempData["Error"] = "El libro no tiene archivos digitales disponibles.";
            return RedirectToAction("Index", "Libros");
        }

        if (diasPrestamo < 2 || diasPrestamo > 20)
        {
            TempData["Error"] = "Días inválidos (2-20).";
            return RedirectToAction("ConfirmarPrestamo", new { id = libroId });
        }

        var tieneMultaPendiente = await _context.Multas
            .Include(m => m.Prestamo)
            .ThenInclude(p => p!.Libro)
            .AnyAsync(m => m.Prestamo != null && m.Prestamo.UsuarioId == usuarioId && !m.Pagada);

        if (tieneMultaPendiente)
        {
            TempData["Error"] = "Tienes multas pendientes.";
            return RedirectToAction("Index", "Libros");
        }

        if (await _context.Prestamos.CountAsync(p => p.UsuarioId == usuarioId && p.FechaDevolucionReal == null) >= 3)
        {
            TempData["Error"] = "Límite de 3 préstamos activos alcanzado.";
            return RedirectToAction("Index", "Libros");
        }

        if (await _context.Prestamos.AnyAsync(p => p.UsuarioId == usuarioId && p.LibroId == libroId && p.FechaDevolucionReal == null))
        {
            TempData["Error"] = "Ya tienes este libro prestado.";
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

        _context.Prestamos.Add(prestamo);
        await _context.SaveChangesAsync();

        string fechaLim = prestamo.FechaDevolucionProgramada.ToShortDateString();
        if (usuarioDB != null)
        {
            NotificarUsuarioSmsAsync(usuarioDB, prestamo, $"Préstamo exitoso. Límite: {fechaLim}.");
            await CrearNotificacionAsync(usuarioDB.Id, "📖 Préstamo Confirmado", $"Has alquilado '{libro.Titulo}'.", "success");
        }

        TempData["Success"] = "Préstamo realizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Vista administrativa para supervisar todos los préstamos globales.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public IActionResult Todos()
    {
        var prestamos = ObtenerPrestamosDetallados().OrderByDescending(p => p.FechaPrestamo).ToList();
        return View(prestamos);
    }

    /// <summary>
    /// Muestra el visor de lectura inmersiva para un libro prestado.
    /// Solo accesible si el préstamo está activo y pertenece al usuario.
    /// Recupera el progreso de lectura guardado anteriormente.
    /// </summary>
    /// <param name="id">ID del préstamo.</param>
    [Authorize(Roles = "Usuario")]
    public async Task<IActionResult> Leer(int id)
    {
        var usuarioId = _userManager.GetUserId(User);
        var prestamo = await _context.Prestamos
            .Include(p => p.Libro)
            .ThenInclude(l => l.Archivos)
            .FirstOrDefaultAsync(p => p.Id == id && p.UsuarioId == usuarioId);

        if (prestamo == null) return NotFound();
        if (prestamo.FechaDevolucionReal != null || prestamo.Estado != "Activo")
        {
            TempData["Error"] = "Préstamo inactivo.";
            return RedirectToAction(nameof(Index));
        }

        if (prestamo.Libro?.Archivos == null || !prestamo.Libro.Archivos.Any())
        {
            TempData["Error"] = "No hay archivos digitales.";
            return RedirectToAction(nameof(Index));
        }

        // Recuperar progreso de lectura
        var progreso = await _context.ProgresosLectura
            .FirstOrDefaultAsync(p => p.UsuarioId == usuarioId && p.LibroId == prestamo.LibroId);

        ViewBag.PaginaGuardada = progreso?.PaginaActual ?? 1;
        ViewBag.PrestamoId = id;

        return View("Leer", prestamo.Libro);
    }

    /// <summary>
    /// Guarda el progreso actual de lectura del usuario (página).
    /// Endpoint consumido vía AJAX desde el visor inmersivo.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Usuario")]
    public async Task<IActionResult> GuardarProgreso(int libroId, int pagina)
    {
        var usuarioId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(usuarioId)) return Unauthorized();

        var progreso = await _context.ProgresosLectura
            .FirstOrDefaultAsync(p => p.UsuarioId == usuarioId && p.LibroId == libroId);

        if (progreso == null)
        {
            progreso = new ProgresoLectura
            {
                UsuarioId = usuarioId,
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
        return Ok(new { success = true, pagina = progreso.PaginaActual });
    }

    /// <summary>
    /// Permite la descarga física de uno de los archivos del libro asociado al préstamo.
    /// Implementa seguridad mediante PhysicalFile para no exponer rutas del servidor.
    /// </summary>
    /// <param name="id">ID del préstamo.</param>
    [Authorize(Roles = "Usuario")]
    public async Task<IActionResult> DescargarLibro(int id)
    {
        var usuarioId = _userManager.GetUserId(User);
        var prestamo = await _context.Prestamos
            .Include(p => p.Libro)
            .ThenInclude(l => l.Archivos)
            .FirstOrDefaultAsync(p => p.Id == id && p.UsuarioId == usuarioId && p.Estado == "Activo" && p.FechaDevolucionReal == null);

        if (prestamo == null || prestamo.Libro?.Archivos == null || !prestamo.Libro.Archivos.Any()) return Forbid();

        // Por ahora descargamos el primer archivo disponible (Modelo simple)
        var archivo = prestamo.Libro.Archivos.First();
        var vaultFolder = Path.Combine(_env.ContentRootPath, "BibliotecaLibros_Vault");
        var filePath = Path.Combine(vaultFolder, archivo.Ruta);

        if (!System.IO.File.Exists(filePath))
        {
            TempData["Error"] = "El archivo no existe en el servidor.";
            return RedirectToAction(nameof(Index));
        }

        await RegistrarAuditoriaAsync("Descarga Física", archivo.LibroId.ToString(), $"Archivo: {archivo.Ruta}");

        return PhysicalFile(filePath, "application/octet-stream", archivo.Ruta);
    }

    /// <summary>
    /// Endpoint para el visor (Iframe) que sirve el contenido del archivo con el Content-Type correcto.
    /// Soporta PDF, EPUB y DOCX mediante streaming seguro.
    /// </summary>
    /// <param name="id">ID del préstamo.</param>
    [Authorize(Roles = "Usuario")]
    public async Task<IActionResult> ArchivoLectura(int id)
    {
        var usuarioId = _userManager.GetUserId(User);
        var prestamo = await _context.Prestamos
            .Include(p => p.Libro)
            .ThenInclude(l => l.Archivos)
            .FirstOrDefaultAsync(p => p.Id == id && p.UsuarioId == usuarioId && p.Estado == "Activo" && p.FechaDevolucionReal == null);

        if (prestamo == null || prestamo.Libro?.Archivos == null || !prestamo.Libro.Archivos.Any()) return Forbid();

        var archivo = prestamo.Libro.Archivos.First();
        var vaultFolder = Path.Combine(_env.ContentRootPath, "BibliotecaLibros_Vault");
        var filePath = Path.Combine(vaultFolder, archivo.Ruta);

        if (!System.IO.File.Exists(filePath)) return NotFound();

        string contentType = "application/octet-stream";
        string ext = Path.GetExtension(filePath).ToLower();
        if (ext == ".pdf") contentType = "application/pdf";
        else if (ext == ".epub") contentType = "application/epub+zip";
        else if (ext == ".docx") contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

        await RegistrarAuditoriaAsync("Lectura Digital", archivo.LibroId.ToString(), $"Formato: {ext}");

        return PhysicalFile(filePath, contentType, enableRangeProcessing: true);
    }
}
