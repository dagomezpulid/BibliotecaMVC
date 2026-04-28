using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaMVC.Controllers
{
    /// <summary>
    /// Gestiona la interacción social de los usuarios con el catálogo.
    /// Permite administrar la lista de libros favoritos y la biblioteca personal.
    /// </summary>
    [Authorize]
    public class FavoritosController : Controller
    {
        private readonly BibliotecaContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// Inicializa una nueva instancia del controlador con inyección de dependencias.
        /// </summary>
        /// <param name="context">Contexto de datos de la biblioteca.</param>
        /// <param name="userManager">Servicio de gestión de identidad de usuarios.</param>
        public FavoritosController(BibliotecaContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Alterna el estado de favorito de un libro para el usuario actual.
        /// Utiliza validación de token Antiforgery para prevenir ataques CSRF en acciones de estado.
        /// </summary>
        /// <param name="libroId">ID único del libro a procesar.</param>
        /// <returns>JSON indicando el éxito y el estado resultante (esFavorito).</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
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
        /// Muestra la colección personal de libros marcados como favoritos por el usuario.
        /// Incluye carga de autores y categorías para la rejilla visual.
        /// </summary>
        /// <returns>Vista con el listado de libros favoritos.</returns>
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var favoritos = await _context.Favoritos
                .Include(f => f.Libro)
                    .ThenInclude(l => l.Autor)
                .Include(f => f.Libro)
                    .ThenInclude(l => l.Categorias)
                .Where(f => f.UsuarioId == userId && f.Libro != null)
                .OrderByDescending(f => f.FechaAgregado)
                .Select(f => f.Libro!)
                .ToListAsync();

            return View(favoritos);
        }
    }
}
