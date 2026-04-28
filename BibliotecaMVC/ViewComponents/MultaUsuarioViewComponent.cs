using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaMVC.ViewComponents
{
    /// <summary>
    /// Componente de vista para la barra de navegación.
    /// Determina si el usuario actual tiene multas pendientes de pago
    /// y expone ese estado como un booleano a su vista parcial.
    /// </summary>
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

        /// <summary>
        /// Ejecuta la lógica del componente: consulta si el usuario tiene multas activas.
        /// </summary>
        /// <returns>Vista parcial con un booleano indicando la presencia de deuda pendiente.</returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!HttpContext.User.Identity!.IsAuthenticated)
                return View(false);

            var userId = _userManager.GetUserId(HttpContext.User);

            bool tieneMulta = await _context.Multas
                .AnyAsync(m =>
                    m.Prestamo != null &&
                    m.Prestamo.UsuarioId == userId &&
                    !m.Pagada);

            return View(tieneMulta);
        }
    }
}
