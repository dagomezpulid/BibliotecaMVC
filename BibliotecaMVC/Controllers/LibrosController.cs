using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using BibliotecaMVC.Services;

namespace BibliotecaMVC.Controllers
{
    /// <summary>
    /// Gestiona el catálogo de libros mediante ILibroService.
    /// </summary>
    public class LibrosController : Controller
    {
        private readonly ILibroService _libroService;
        private readonly BibliotecaContext _context; // Todavía usado para Favoritos (podría moverse a IFavoritosService luego)

        public LibrosController(ILibroService libroService, BibliotecaContext context)
        {
            _libroService = libroService;
            _context = context;
        }

        /// <summary>
        /// Muestra el catálogo de libros con capacidades de búsqueda y paginación.
        /// </summary>
        [Authorize]
        public async Task<IActionResult> Index(string? query, int page = 1)
        {
            int pageSize = 8;
            var (libros, totalPages) = await _libroService.GetPagedLibrosAsync(query, page, pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Query = query;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                var favoritosIds = await _context.Favoritos
                    .Where(f => f.UsuarioId == userId)
                    .Select(f => f.LibroId)
                    .ToListAsync();

                foreach (var l in libros)
                {
                    if (favoritosIds.Contains(l.Id)) l.EsFavorito = true;
                }
            }

            ViewBag.Categorias = await _context.Categorias.OrderBy(c => c.Nombre).ToListAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_LibrosGrid", libros);
            }

            return View(libros);
        }

        /// <summary>
        /// Muestra la información técnica completa del libro.
        /// </summary>
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var libro = await _libroService.GetLibroDetailsAsync(id.Value);
            if (libro == null) return NotFound();

            var recomendados = await _libroService.GetRecommendedLibrosAsync(
                libro.Id, 
                libro.AutorId, 
                libro.Categorias.Select(c => c.Id).ToList()
            );

            ViewBag.Recomendados = recomendados;
            return View(libro);
        }

        /// <summary>
        /// Procesa la publicación de una nueva reseña.
        /// </summary>
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostResena(int LibroId, int Puntuacion, string Comentario)
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(usuarioId)) return Unauthorized();

            if (await _context.Resenas.AnyAsync(r => r.LibroId == LibroId && r.UsuarioId == usuarioId))
            {
                TempData["Error"] = "Ya has calificado este libro.";
                return RedirectToAction(nameof(Details), new { id = LibroId });
            }

            await _libroService.PostResenaAsync(LibroId, usuarioId, Puntuacion, Comentario);
            TempData["Success"] = "¡Reseña publicada!";
            
            return RedirectToAction(nameof(Details), new { id = LibroId });
        }

        /// <summary>
        /// Muestra el formulario de creación.
        /// </summary>
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewBag.Autores = new SelectList(_context.Autores, "Id", "Nombre");
            ViewBag.Categorias = new MultiSelectList(_context.Categorias, "Id", "Nombre");
            return View();
        }

        /// <summary>
        /// Procesa la creación de un libro.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Titulo,AutorId,ISBN,ImagenUrl,Descripcion")] Libro libro, int[] CategoriasSeleccionadas, IFormFileCollection archivosLibro)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Autores = new SelectList(_context.Autores, "Id", "Nombre", libro.AutorId);
                ViewBag.Categorias = new MultiSelectList(_context.Categorias, "Id", "Nombre", CategoriasSeleccionadas);
                return View(libro);
            }

            var success = await _libroService.CreateLibroAsync(libro, CategoriasSeleccionadas, archivosLibro);
            if (!success)
            {
                ModelState.AddModelError("Titulo", "Error al crear el libro o el título ya existe.");
                ViewBag.Autores = new SelectList(_context.Autores, "Id", "Nombre", libro.AutorId);
                ViewBag.Categorias = new MultiSelectList(_context.Categorias, "Id", "Nombre", CategoriasSeleccionadas);
                return View(libro);
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Muestra el formulario de edición.
        /// </summary>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var libro = await _context.Libros.Include(l => l.Categorias).Include(l => l.Archivos).FirstOrDefaultAsync(l => l.Id == id);
            if (libro == null) return NotFound();

            ViewBag.Autores = new SelectList(_context.Autores, "Id", "Nombre", libro.AutorId);
            ViewBag.Categorias = new MultiSelectList(_context.Categorias, "Id", "Nombre", libro.Categorias.Select(c => c.Id));
            return View(libro);
        }

        /// <summary>
        /// Procesa la actualización de un libro.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Titulo,AutorId,ISBN,ImagenUrl,Descripcion")] Libro libro, int[] CategoriasSeleccionadas, IFormFileCollection nuevosArchivos)
        {
            if (id != libro.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var success = await _libroService.UpdateLibroAsync(libro, CategoriasSeleccionadas, nuevosArchivos);
                if (success) return RedirectToAction(nameof(Index));
            }
            
            ViewBag.Autores = new SelectList(_context.Autores, "Id", "Nombre", libro.AutorId);
            ViewBag.Categorias = new MultiSelectList(_context.Categorias, "Id", "Nombre", CategoriasSeleccionadas);
            return View(libro);
        }

        /// <summary>
        /// Muestra la confirmación de eliminación.
        /// </summary>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var libro = await _context.Libros.Include(l => l.Autor).FirstOrDefaultAsync(m => m.Id == id);
            if (libro == null) return NotFound();
            return View(libro);
        }

        /// <summary>
        /// Realiza la eliminación de un libro.
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var success = await _libroService.DeleteLibroAsync(id);
            if (success) TempData["Success"] = "Libro y archivos eliminados.";
            
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Acceso directo a la pasarela de préstamo.
        /// </summary>
        [Authorize]
        public async Task<IActionResult> Prestar(int id)
        {
            var libro = await _context.Libros.Include(l => l.Archivos).FirstOrDefaultAsync(l => l.Id == id);
            if (libro == null) return NotFound();
            if (!libro.Archivos.Any())
            {
                TempData["Error"] = "Libro sin archivos digitales.";
                return RedirectToAction("Index", "Libros");
            }
            ViewBag.LibroTitulo = libro.Titulo;
            return View(new Prestamo { LibroId = id });
        }
    }
}
