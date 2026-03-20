using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class MultasController : Controller
{
    private readonly BibliotecaContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public MultasController(BibliotecaContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [Authorize(Roles = "Usuario")]
    public async Task<IActionResult> MisMultas()
    {
        var usuarioId = _userManager.GetUserId(User);

        var multas = await _context.Multas
            .Include(m => m.Prestamo)
            .ThenInclude(p => p.Libro)
            .Where(m => m.Prestamo.UsuarioId == usuarioId)
            .OrderByDescending(m => m.FechaGenerada)
            .ToListAsync();

        return View(multas);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index()
    {
        var multas = await _context.Multas
            .Include(m => m.Prestamo)
            .ThenInclude(p => p.Libro)
            .Include(m => m.Prestamo)
            .ThenInclude(p => p.Usuario)
            .OrderByDescending(m => m.FechaGenerada)
            .ToListAsync();

        return View(multas);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> MarcarPagada(int id)
    {
        var multa = await _context.Multas.FindAsync(id);

        if (multa == null)
            return NotFound();

        multa.Pagada = true;
        multa.FechaPago = DateTime.Now;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Multa pagada correctamente.";

        return RedirectToAction(nameof(Index));
    }



}