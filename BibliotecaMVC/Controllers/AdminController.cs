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

    public async Task<IActionResult> Usuarios()
    {
        var usuarios = await _userManager.Users.ToListAsync();

        var model = new List<UserViewModel>();

        foreach (var user in usuarios)
        {
            var roles = await _userManager.GetRolesAsync(user);

            model.Add(new UserViewModel
            {
                Id = user.Id,
                NombreCompleto = user.Nombre + " " + user.Apellido,
                Email = user.Email!,
                Roles = roles.ToList()
            });
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> HacerAdmin(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        if (!await _userManager.IsInRoleAsync(user, "Admin"))
        {
            await _userManager.AddToRoleAsync(user, "Admin");
        }

        return RedirectToAction(nameof(Usuarios));
    }

    [HttpPost]
    public async Task<IActionResult> QuitarAdmin(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            await _userManager.RemoveFromRoleAsync(user, "Admin");
        }

        return RedirectToAction(nameof(Usuarios));
    }

    [HttpPost]
    public async Task<IActionResult> Bloquear(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);
        await _userManager.UpdateAsync(user);

        return RedirectToAction(nameof(Usuarios));
    }

    [HttpPost]
    public async Task<IActionResult> Desbloquear(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        user.LockoutEnd = null;
        await _userManager.UpdateAsync(user);

        return RedirectToAction(nameof(Usuarios));
    }

    [HttpPost]
    public async Task<IActionResult> Eliminar(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        await _userManager.DeleteAsync(user);

        return RedirectToAction(nameof(Usuarios));
    }
}
