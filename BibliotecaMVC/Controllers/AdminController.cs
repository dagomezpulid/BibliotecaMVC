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
        var users = _userManager.Users.ToList();

        foreach (var user in users)
        {
            await ActualizarEstadoBloqueo(user);
        }

        var viewModel = new List<UserViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            viewModel.Add(new UserViewModel
            {
                Id = user.Id,
                NombreCompleto = user.NombreCompleto,
                Email = user.Email,
                Roles = roles.ToList(),
                EstaBloqueado = user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.Now
            });
        }

        return View(viewModel);
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
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarUsuario(string id)
    {
        var usuario = await _userManager.FindByIdAsync(id);

        if (usuario == null)
            return NotFound();

        if (usuario.Id == _userManager.GetUserId(User))
        {
            TempData["Error"] = "No puedes eliminar tu propio usuario.";
            return RedirectToAction("Usuarios");
        }

        if (usuario.Email == "admin@biblioteca.com")
        {
            TempData["Error"] = "No se puede eliminar el administrador principal.";
            return RedirectToAction("Usuarios");
        }

        var tienePrestamos = _context.Prestamos.Any(p => p.UsuarioId == usuario.Id && !p.Devuelto);

        if (tienePrestamos)
        {
            TempData["Error"] = "No se puede eliminar un usuario con préstamos activos.";
            return RedirectToAction("Usuarios");
        }

        await _userManager.DeleteAsync(usuario);

        TempData["Success"] = "Usuario eliminado correctamente.";
        return RedirectToAction("Usuarios");
    }

    private async Task<bool> DebeEstarBloqueado(ApplicationUser user)
    {
        var prestamos = await _context.Prestamos
            .Where(p => p.UsuarioId == user.Id && !p.Devuelto)
            .ToListAsync();

        foreach (var prestamo in prestamos)
        {
            if (DateTime.Now > prestamo.FechaDevolucion)
            {
                var diasMora = (DateTime.Now - prestamo.FechaDevolucion!.Value).Days;

                if (diasMora >= 8)
                    return true;
            }
        }

        return false;
    }

    private async Task ActualizarEstadoBloqueo(ApplicationUser user)
    {
        bool bloquear = await DebeEstarBloqueado(user);

        if (bloquear)
        {
            user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);
        }
        else
        {
            user.LockoutEnd = null;
        }

        await _userManager.UpdateAsync(user);
    }
}
