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

    public IActionResult Index()
    {
        var prestamos = _context.Prestamos
            .Include(p => p.Libro)
            .ToList();

        return View(prestamos);
    }

    // GET: Prestamos/Devolver
    public IActionResult Devolver(int id)
    {
        var prestamo = _context.Prestamos
            .Include(p => p.Libro)
            .FirstOrDefault(p => p.PrestamoID == id);

        if (prestamo == null)
            return NotFound();

        return View(prestamo);
    }

    // POST: Prestamos/Devolver
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DevolverConfirmado(int id)
    {
        var prestamo = _context.Prestamos
            .Include(p => p.Libro)
            .FirstOrDefault(p => p.PrestamoID == id);

        if (prestamo == null)
            return NotFound();

        prestamo.Libro.Stock += 1;

        _context.Prestamos.Remove(prestamo);
        _context.SaveChanges();

        return RedirectToAction("Index");
    }

}

