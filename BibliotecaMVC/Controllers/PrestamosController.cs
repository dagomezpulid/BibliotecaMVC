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

    // Préstamos activos
    public IActionResult Index()
    {
        var usuarioId = _userManager.GetUserId(User);

        var prestamos = _context.Prestamos
            .Include(p => p.Libro)
            .Include(p => p.Usuario)
            .Include(p => p.Multa)
            .Where(p => p.UsuarioId == usuarioId &&
                p.FechaDevolucionReal == null)
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

    // Devolver libro
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Usuario")]
    public async Task<IActionResult> Devolver(int id)
    {
        var prestamo = await _context.Prestamos
            .Include(p => p.Libro)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (prestamo == null)
            return NotFound();

        prestamo.FechaDevolucionReal = DateTime.Now;
        prestamo.Estado = "Devuelto";

        // Generar multa si aplica
        if (prestamo.FechaDevolucionReal > prestamo.FechaDevolucionProgramada)
        {
            var diasMora =
                (prestamo.FechaDevolucionReal.Value -
                 prestamo.FechaDevolucionProgramada).Days;

            decimal valorPorDia = 1000;
            decimal totalMulta = diasMora * valorPorDia;

            var multa = new Multa
            {
                PrestamoId = prestamo.Id,
                Monto = totalMulta,
                Pagada = false,
                FechaGenerada = DateTime.Now
            };

            _context.Multas.Add(multa);
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

    // Crear préstamo
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Usuario")]
    public async Task<IActionResult> Prestar(int libroId)
    {
        var usuarioId = _userManager.GetUserId(User);

        var libro = await _context.Libros.FindAsync(libroId);

        if (libro == null || libro.Stock <= 0)
            return BadRequest("Libro no disponible");

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
            FechaDevolucionProgramada = DateTime.Now.AddDays(7),
            Estado = "Activo"
        };

        libro.Stock--;

        _context.Prestamos.Add(prestamo);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Préstamo realizado correctamente.";

        return RedirectToAction(nameof(Index));
    }

    // Vista admin
    [Authorize(Roles = "Admin")]
    public IActionResult Todos()
    {
        var prestamos = _context.Prestamos
            .Include(p => p.Libro)
            .Include(p => p.Usuario)
            .Include(p => p.Multa)
            .OrderByDescending(p => p.FechaPrestamo)
            .ToList();

        return View(prestamos);
    }
}


