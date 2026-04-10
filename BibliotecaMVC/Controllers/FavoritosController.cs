using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaMVC.Controllers
{
    [Authorize]
    public class FavoritosController : Controller
    {
        private readonly BibliotecaContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FavoritosController(BibliotecaContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Alterna el estado de favorito de un libro para el usuario actual.
        /// Diseñado para interacción mediante AJAX.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Toggle(int libroId)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var favorito = await _context.Favoritos
                .FirstOrDefaultAsync(f => f.LibroId == libroId && f.UsuarioId == userId);

            bool esFavorito;

            if (favorito != null)
            {
                _context.Favoritos.Remove(favorito);
                esFavorito = false;
            }
            else
            {
                _context.Favoritos.Add(new Favorito
                {
                    LibroId = libroId,
                    UsuarioId = userId
                });
                esFavorito = true;
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, esFavorito });
        }

        /// <summary>
        /// Vista de la biblioteca personal del usuario.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var favoritos = await _context.Favoritos
                .Include(f => f.Libro)
                    .ThenInclude(l => l.Autor)
                .Include(f => f.Libro)
                    .ThenInclude(l => l.Categorias)
                .Where(f => f.UsuarioId == userId)
                .OrderByDescending(f => f.FechaAgregado)
                .Select(f => f.Libro)
                .ToListAsync();

            return View(favoritos);
        }
    }
}
