using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using BibliotecaMVC.Services;
using Microsoft.Extensions.Logging;

namespace BibliotecaMVC.Controllers
{
    /// <summary>
    /// Controlador principal para la gestión de préstamos.
    /// Utiliza el servicio IPrestamoService para desacoplar la lógica de negocio.
    /// </summary>
    [Authorize]
    public class PrestamosController : Controller
    {
        private readonly IPrestamoService _prestamoService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly BibliotecaContext _context; // Todavía necesario para Auditoría directa y algunos joins específicos si no están en servicio
        private readonly ILogger<PrestamosController> _logger;

        public PrestamosController(
            IPrestamoService prestamoService,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env,
            BibliotecaContext context,
            ILogger<PrestamosController> logger)
        {
            _prestamoService = prestamoService;
            _userManager = userManager;
            _env = env;
            _context = context;
            _logger = logger;
        }

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar auditoría");
            }
        }

        /// <summary>
        /// Muestra los préstamos activos del usuario autenticado.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var usuarioId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(usuarioId)) return Unauthorized();
            
            var prestamos = await _prestamoService.GetActiveLoansAsync(usuarioId);
            return View(prestamos);
        }

        /// <summary>
        /// Muestra el histórico completo de préstamos (devueltos y activos) del usuario.
        /// </summary>
        public async Task<IActionResult> Historial()
        {
            var usuarioId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(usuarioId)) return Unauthorized();

            var historial = await _prestamoService.GetLoanHistoryAsync(usuarioId);
            return View(historial);
        }

        /// <summary>
        /// Procesa la devolución de un libro digital.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> Devolver(int id)
        {
            var usuarioId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(usuarioId)) return Unauthorized();

            var result = await _prestamoService.ProcessReturnAsync(usuarioId, id);
            
            if (result.Success)
            {
                TempData["Success"] = result.Message;
                if (result.Message.Contains("SUSPENDIDA")) TempData["Error"] = result.Message; // Resaltar si hay bloqueo
            }
            else
            {
                TempData["Error"] = result.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Paso previo al préstamo: Muestra la pasarela de selección de días.
        /// </summary>
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> ConfirmarPrestamo(int id)
        {
            var libro = await _context.Libros
                .Include(l => l.Autor)
                .Include(l => l.Categorias)
                .Include(l => l.Archivos)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (libro == null) return NotFound();

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
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> Prestar(int libroId, int diasPrestamo)
        {
            var usuarioId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(usuarioId)) return Unauthorized();

            var result = await _prestamoService.ProcessLoanAsync(usuarioId, libroId, diasPrestamo);

            if (result.Success)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["Error"] = result.Message;
                // Si el error es por días inválidos, volver a la confirmación
                if (result.Message.Contains("Días")) 
                    return RedirectToAction("ConfirmarPrestamo", new { id = libroId });
                
                return RedirectToAction("Index", "Libros");
            }
        }

        /// <summary>
        /// Vista administrativa para supervisar todos los préstamos globales.
        /// </summary>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Todos()
        {
            var prestamos = await _prestamoService.GetAllLoansAsync();
            return View(prestamos);
        }

        /// <summary>
        /// Muestra el visor de lectura inmersiva.
        /// </summary>
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> Leer(int id)
        {
            var usuarioId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(usuarioId)) return Unauthorized();

            var prestamo = await _context.Prestamos
                .Include(p => p.Libro)
                .ThenInclude(l => l!.Archivos)
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

            var progreso = await _prestamoService.GetReadingProgressAsync(usuarioId, prestamo.LibroId);

            ViewBag.PaginaGuardada = progreso?.PaginaActual ?? 1;
            ViewBag.PrestamoId = id;

            return View("Leer", prestamo.Libro);
        }

        /// <summary>
        /// Guarda el progreso actual de lectura del usuario.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> GuardarProgreso(int libroId, int pagina)
        {
            var usuarioId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(usuarioId)) return Unauthorized();

            await _prestamoService.SaveReadingProgressAsync(usuarioId, libroId, pagina);
            return Ok(new { success = true });
        }

        /// <summary>
        /// Permite la descarga física de uno de los archivos.
        /// </summary>
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> DescargarLibro(int id)
        {
            var usuarioId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(usuarioId)) return Unauthorized();

            var prestamo = await _context.Prestamos
                .Include(p => p.Libro)
                .ThenInclude(l => l!.Archivos)
                .FirstOrDefaultAsync(p => p.Id == id && p.UsuarioId == usuarioId && p.Estado == "Activo" && p.FechaDevolucionReal == null);

            if (prestamo == null || prestamo.Libro?.Archivos == null || !prestamo.Libro.Archivos.Any()) return Forbid();

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
        /// Endpoint para el visor (Iframe) que sirve el contenido del archivo.
        /// </summary>
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> ArchivoLectura(int id)
        {
            var usuarioId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(usuarioId)) return Unauthorized();

            var prestamo = await _context.Prestamos
                .Include(p => p.Libro)
                .ThenInclude(l => l!.Archivos)
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
}
