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
    public async Task<IActionResult> Index()
    {
        // 1. Cargar usuarios
        var users = await _userManager.Users.ToListAsync();
        var usuariosIds = users.Select(u => u.Id).ToList();

        // N+1 SOLVED: Cargar todos los préstamos activos globalmente
        var prestamosActivosAll = await _context.Prestamos
            .Where(p => p.FechaDevolucionReal == null && usuariosIds.Contains(p.UsuarioId))
            .ToListAsync();

        var prestamosPorUsuario = prestamosActivosAll
            .GroupBy(p => p.UsuarioId!)
            .ToDictionary(g => g.Key, g => g.ToList());

        // 2. Preparar el listado visual
        var userViewModels = new List<UserViewModel>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            userViewModels.Add(new UserViewModel
            {
                Id = user.Id,
                NombreCompleto = user.NombreCompleto,
                Email = user.Email,
                Roles = roles.ToList(),
                EstaBloqueado = user.BloqueadoParaPrestamos
            });
        }

        // 3. Preparar el modelo general del Dashboard
        var model = new AdminDashboardViewModel
        {
            TotalUsuarios = users.Count,
            TotalLibros = await _context.Libros.CountAsync(),
            TotalAutores = await _context.Autores.CountAsync(),
            PrestamosActivos = prestamosActivosAll.Count,
            TotalMultasPendientes = await _context.Multas
                        .Where(m => !m.Pagada)
                        .SumAsync(m => (decimal?)m.Monto) ?? 0,
            Usuarios = userViewModels
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> HacerAdmin(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        if (!await _userManager.IsInRoleAsync(user, "Admin"))
        {
            await _userManager.AddToRoleAsync(user, "Admin");
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuitarAdmin(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            await _userManager.RemoveFromRoleAsync(user, "Admin");
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DesbloquearUsuario(string id)
    {
        var usuario = await _userManager.FindByIdAsync(id);
        if (usuario == null) return NotFound();

        usuario.BloqueadoParaPrestamos = false;
        await _userManager.UpdateAsync(usuario);

        TempData["Success"] = $"El usuario {usuario.NombreCompleto} ha sido rehabilitado exitosamente para nuevos préstamos.";
        return RedirectToAction(nameof(Index));
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
            return RedirectToAction(nameof(Index));
        }

        if (usuario.Email == "admin@biblioteca.com")
        {
            TempData["Error"] = "No se puede eliminar el administrador principal.";
            return RedirectToAction(nameof(Index));
        }

        var tienePrestamosActivos = _context.Prestamos.Any(p => p.UsuarioId == usuario.Id && p.FechaDevolucionReal == null);

        if (tienePrestamosActivos)
        {
            TempData["Error"] = "No se puede eliminar un usuario con préstamos activos.";
            return RedirectToAction(nameof(Index));
        }

        // Eliminar dependencias primero (Historial de préstamos inactivos y sus multas)
        var historialPrestamos = await _context.Prestamos
                                    .Where(p => p.UsuarioId == usuario.Id)
                                    .ToListAsync();

        if (historialPrestamos.Any())
        {
            var prestamosIds = historialPrestamos.Select(p => p.Id).ToList();
            var multas = await _context.Multas
                            .Where(m => prestamosIds.Contains(m.PrestamoId))
                            .ToListAsync();

            _context.Multas.RemoveRange(multas);
            _context.Prestamos.RemoveRange(historialPrestamos);
            await _context.SaveChangesAsync(); // Guardamos los cambios de eliminación en cascada
        }

        await _userManager.DeleteAsync(usuario);

        TempData["Success"] = "Usuario y su historial han sido eliminados correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // Métodos aislados removidos para delegar al Modelo (Clean Code)
}