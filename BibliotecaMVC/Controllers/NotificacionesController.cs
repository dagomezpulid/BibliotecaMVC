using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BibliotecaMVC.Controllers
{
    [Authorize]
    public class NotificacionesController : Controller
    {
        private readonly BibliotecaContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificacionesController(BibliotecaContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Obtiene las últimas notificaciones no leídas del usuario actual.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetRecientes()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var notificaciones = await _context.Notificaciones
                .Where(n => n.UsuarioId == userId)
                .OrderByDescending(n => n.FechaCreacion)
                .Take(5)
                .ToListAsync();

            return Json(notificaciones);
        }

        /// <summary>
        /// Marca una notificación específica como leída.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> MarcarLeida(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var notif = await _context.Notificaciones.FindAsync(id);

            if (notif == null || notif.UsuarioId != userId) return NotFound();

            notif.Leida = true;
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        /// <summary>
        /// Obtiene el conteo de notificaciones no leídas para el badge del navbar.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetConteo()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Json(0);

            var conteo = await _context.Notificaciones
                .CountAsync(n => n.UsuarioId == userId && !n.Leida);

            return Json(conteo);
        }
    }
}
