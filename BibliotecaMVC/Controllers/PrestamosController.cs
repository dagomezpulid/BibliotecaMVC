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
    private readonly UserManager<IdentityUser> _userManager;

    public PrestamosController(
    BibliotecaContext context,
    UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Prestamos
    public IActionResult Index()
    {
        var usuarioId = _userManager.GetUserId(User);

        var prestamos = _context.Prestamos
            .Include(p => p.Libro)
            .Where(p => !p.Devuelto && p.UsuarioId == usuarioId)
            .ToList();

        return View(prestamos);
    }

    // POST: Prestamos/Devolver
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Devolver(int id)
    {
        var prestamo = _context.Prestamos
            .Include(p => p.Libro)
            .FirstOrDefault(p => p.PrestamoID == id);

        if (prestamo == null || prestamo.Devuelto)
            return NotFound();

        prestamo.Devuelto = true;
        prestamo.FechaDevolucionReal = DateTime.Now;

        if (prestamo.FechaDevolucionReal > prestamo.FechaDevolucion)
        {
            var fechaLimite = prestamo.FechaDevolucion.Value.Date;
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

        prestamo.Libro.Stock += 1;
        _context.SaveChanges();

        return RedirectToAction(nameof(Index));
    }

    // GET: Prestamos/Historial
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

    // POST: Prestamos/Prestar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Prestar(Prestamo prestamo)
    {
        return Content("ENTRÓ A PRESTAR");

        if (!User.Identity.IsAuthenticated)
            return Unauthorized();

        if (!ModelState.IsValid)
            return Content("ModelState inválido");

        var usuarioId = _userManager.GetUserId(User);

        if (string.IsNullOrEmpty(usuarioId))
            return Content("UsuarioId NULL");

        var libro = await _context.Libros.FindAsync(prestamo.LibroID);

        if (libro == null)
            return Content("Libro no encontrado");

        var nuevoPrestamo = new Prestamo
        {
            LibroID = prestamo.LibroID,
            NombreSolicitante = prestamo.NombreSolicitante,
            FechaPrestamo = DateTime.Now,
            FechaDevolucion = prestamo.FechaDevolucion,
            UsuarioId = usuarioId,
            Devuelto = false
        };

        libro.Stock--;

        _context.Prestamos.Add(nuevoPrestamo);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // GET MisPrestamos
    [Authorize]
    public IActionResult MisPrestamos()
    {
        var usuarioId = _userManager.GetUserId(User);

        var prestamos = _context.Prestamos
            .Include(p => p.Libro)
            .Where(p => p.UsuarioId == usuarioId)
            .OrderByDescending(p => p.FechaPrestamo)
            .ToList();

        return View(prestamos);
    }


}

