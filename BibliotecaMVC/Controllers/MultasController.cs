using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin")]
public class MultasController : Controller
{
    private readonly BibliotecaContext _context;

    public MultasController(BibliotecaContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var multas = await _context.Multas
            .Include(m => m.Prestamo)
            .ThenInclude(p => p.Usuario)
            .Include(m => m.Prestamo.Libro)
            .OrderByDescending(m => m.FechaGenerada)
            .ToListAsync();

        return View(multas);
    }

    [HttpPost]
    public async Task<IActionResult> MarcarPagada(int id)
    {
        var multa = await _context.Multas.FindAsync(id);

        if (multa == null)
            return NotFound();

        multa.Pagada = true;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Multa pagada correctamente.";

        return RedirectToAction(nameof(Index));
    }
}