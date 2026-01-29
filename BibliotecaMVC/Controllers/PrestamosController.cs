using BibliotecaMVC;
using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class PrestamosController : Controller
{
    private readonly BibliotecaContext _context;

    public PrestamosController(BibliotecaContext context)
    {
        _context = context;
    }

    // GET: Prestamos
    public IActionResult Index()
    {
        var prestamos = _context.Prestamos
            .Include(p => p.Libro)
            .Where(p => !p.Devuelto)
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
        var historial = _context.Prestamos
            .Include(p => p.Libro)
            .Where(p => p.Devuelto)
            .OrderByDescending(p => p.FechaDevolucionReal)
            .ToList();

        return View(historial);
    }
}

