using BibliotecaMVC;
using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

[Authorize]
public class PrestamosController : Controller
{
    private readonly BibliotecaContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public PrestamosController(
        BibliotecaContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // 📚 Préstamos activos
    public IActionResult Index()
    {
        var usuarioId = _userManager.GetUserId(User);

        var prestamos = _context.Prestamos
            .Include(p => p.Libro)
            .Where(p => !p.Devuelto && p.UsuarioId == usuarioId)
            .ToList();

        return View(prestamos);
    }

    // 📚 Historial
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

    // 📚 Devolver libro
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Usuario")]
    public IActionResult Devolver(int id)
    {
        var prestamo = _context.Prestamos
            .Include(p => p.Libro)
            .FirstOrDefault(p => p.Id == id);

        if (prestamo == null || prestamo.Devuelto)
            return NotFound();

        prestamo.Devuelto = true;
        prestamo.FechaDevolucionReal = DateTime.Now;

        if (prestamo.FechaDevolucionReal > prestamo.FechaDevolucion)
        {
            var fechaLimite = prestamo.FechaDevolucion!.Value.Date;
            var fechaReal = prestamo.FechaDevolucionReal.Value.Date;

            prestamo.DiasRetraso = (fechaReal - fechaLimite).Days;

            const decimal valorPorDia = 2000;
            prestamo.Multa = prestamo.DiasRetraso * valorPorDia;
        }
        else
        {
            prestamo.DiasRetraso = 0;
            prestamo.Multa = 0;
        }

        prestamo.Libro.Stock++;

        _context.SaveChanges();

        return RedirectToAction(nameof(Index));
    }

    // 📚 Crear préstamo
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Usuario")]
    public async Task<IActionResult> Prestar(Prestamo prestamo)
    {
        if (!User.Identity!.IsAuthenticated)
            return Unauthorized();

        var usuarioId = _userManager.GetUserId(User);

        if (string.IsNullOrEmpty(usuarioId))
            return BadRequest("UsuarioId no encontrado");

        var libro = await _context.Libros.FindAsync(prestamo.LibroId);

        if (libro == null || libro.Stock <= 0)
            return BadRequest("Libro no disponible");

        // 🔒 Validar multa antes
        var tieneMulta = _context.Prestamos.Any(p =>
            p.UsuarioId == usuarioId &&
            !p.Devuelto &&
            p.Multa > 0);

        if (tieneMulta)
        {
            TempData["Error"] = "No puedes realizar préstamos mientras tengas multas pendientes.";
            return RedirectToAction("Index", "Libros");
        }

        prestamo.UsuarioId = usuarioId;
        prestamo.FechaPrestamo = DateTime.Now;
        prestamo.Devuelto = false;
        prestamo.DiasRetraso = 0;
        prestamo.Multa = 0;

        libro.Stock--;

        _context.Prestamos.Add(prestamo);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Préstamo realizado correctamente.";

        return RedirectToAction(nameof(Index));
    }

    // 📚 Mis préstamos (redundante pero útil)
    [Authorize(Roles = "Usuario")]
    public IActionResult MisPrestamos()
    {
        var usuarioId = _userManager.GetUserId(User);

        var prestamos = _context.Prestamos
            .Include(p => p.Libro)
            .Include(p => p.Usuario)
            .OrderByDescending(p => p.FechaPrestamo)
            .ToList();
        return View(prestamos);
    }

    // 📚 Vista admin
    [Authorize(Roles = "Admin")]
    public IActionResult Todos()
    {
        var prestamos = _context.Prestamos
            .Include(p => p.Libro)
            .Include(p => p.Usuario)
            .OrderByDescending(p => p.FechaPrestamo)
            .ToList();

        return View(prestamos);
    }
}


