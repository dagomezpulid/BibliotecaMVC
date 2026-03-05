using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaMVC.ViewComponents
{
    public class MultaUsuarioViewComponent : ViewComponent
    {
        private readonly BibliotecaContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MultaUsuarioViewComponent(
            BibliotecaContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!HttpContext.User.Identity!.IsAuthenticated)
                return View(false);

            var userId = _userManager.GetUserId(HttpContext.User);

            bool tieneMulta = await _context.Multas
                .Include(m => m.Prestamo)
                .AnyAsync(m =>
                    m.Prestamo.UsuarioId == userId &&
                    !m.Pagada);

            return View(tieneMulta);
        }
    }
}
