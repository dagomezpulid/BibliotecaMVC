using BibliotecaMVC.Models;
using BibliotecaMVC.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        var users = _userManager.Users.ToList();
        return View(users);
    }
    //ToggleAdmin
    public async Task<IActionResult> ToggleAdmin()
    {
        var users = _userManager.Users.ToList();

        var model = new List<UserViewModel>();

        foreach (var user in users)
        {
            model.Add(new UserViewModel
            {
                Id = user.Id,
                NombreCompleto = user.NombreCompleto,
                Email = user.Email!,
                EsAdmin = await _userManager.IsInRoleAsync(user, "Admin")
            });
        }

        return View(model);
    }
}
