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

            // REFUERZO DE SEGURIDAD: Solo permitir reseñas de usuarios que han leído el libro (Préstamo previo)
            var haPrestado = await _context.Prestamos.AnyAsync(p => p.LibroId == LibroId && p.UsuarioId == usuarioId);
            if (!haPrestado)
            {
                TempData["Error"] = "Para calificar este libro, primero debes solicitarlo en préstamo y disfrutar de su lectura.";
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

            var result = await _libroService.CreateLibroAsync(libro, CategoriasSeleccionadas, archivosLibro);
            if (!result.Success)
            {
                TempData["Error"] = result.ErrorMessage;
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
                var result = await _libroService.UpdateLibroAsync(libro, CategoriasSeleccionadas, nuevosArchivos);
                if (result.Success) return RedirectToAction(nameof(Index));
                
                TempData["Error"] = result.ErrorMessage;
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
        /// <summary>
        /// Proxy del servidor para enriquecer metadatos de libros (Google Books / OpenLibrary).
        /// Protege la privacidad del usuario al no realizar peticiones directas desde su navegador.
        /// </summary>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Enriquecer(string isbn)
        {
            if (string.IsNullOrEmpty(isbn)) return BadRequest();
            
            // Limpiar ISBN
            isbn = new string(isbn.Where(char.IsLetterOrDigit).ToArray()).ToUpper();
            
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);

            // 1. Intentar Google Books
            try
            {
                var googleRes = await client.GetAsync($"https://www.googleapis.com/books/v1/volumes?q=isbn:{isbn}");
                if (googleRes.IsSuccessStatusCode)
                {
                    var data = await googleRes.Content.ReadFromJsonAsync<dynamic>();
                    if (data?.items != null && data?.items?.Count > 0)
                    {
                        var info = data?.items?[0]?.volumeInfo;
                        string? desc = info?.description;
                        string? img = info?.imageLinks?.thumbnail;
                        
                        return Json(new { 
                            description = desc, 
                            imagenUrl = img?.Replace("http://", "https://"),
                            source = "Google Books"
                        });
                    }
                }
            }
            catch { /* Continuar al siguiente proveedor */ }

            // 2. Intentar OpenLibrary
            try
            {
                var olRes = await client.GetAsync($"https://openlibrary.org/api/books?bibkeys=ISBN:{isbn}&format=json&jscmd=data");
                if (olRes.IsSuccessStatusCode)
                {
                    var data = await olRes.Content.ReadFromJsonAsync<Dictionary<string, dynamic>>();
                    var key = $"ISBN:{isbn}";
                    if (data != null && data.ContainsKey(key))
                    {
                        var book = data[key];
                        string? desc = book?.notes;
                        string? img = book?.cover?.large;
                        
                        return Json(new { 
                            description = desc, 
                            imagenUrl = img,
                            source = "OpenLibrary"
                        });
                    }
                }
            }
            catch { }

            return NotFound();
        }
    }
}
