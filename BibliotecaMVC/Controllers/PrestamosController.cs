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
            .Where(p => !p.Devuelto)
            .ToList();

        return View(prestamos);
    }

    // GET: Prestamos/Devolver
    [HttpPost]
    public IActionResult Devolver(int id)
    {
        var prestamo = _context.Prestamos
            .Include(p => p.Libro)
            .FirstOrDefault(p => p.PrestamoID == id);

        if (prestamo == null || prestamo.Devuelto)
        {
            return NotFound();
        }

        prestamo.Devuelto = true;
        prestamo.FechaDevolucionReal = DateTime.Now;
        prestamo.Libro.Stock += 1;

        _context.SaveChanges();

        return RedirectToAction("Index");
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

