using BibliotecaMVC.Models;
using BibliotecaMVC.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly BibliotecaContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(BibliotecaContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }
    //ToggleAdmin
    public async Task<IActionResult> Index()
    {
        var model = new AdminDashboardViewModel
        {
            TotalUsuarios = _userManager.Users.Count(),
            TotalLibros = await _context.Libros.CountAsync(),
            TotalAutores = await _context.Autores.CountAsync(),
            PrestamosActivos = await _context.Prestamos
                        .Where(p => p.FechaDevolucion == null)
                        .CountAsync(),
            TotalMultasPendientes = await _context.Prestamos
                        .Where(p => p.Multa > 0)
                        .SumAsync(p => (decimal?)p.Multa) ?? 0
        };

        return View(model);
    }
}
