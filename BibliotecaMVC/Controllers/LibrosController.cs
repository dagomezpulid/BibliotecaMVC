using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace BibliotecaMVC.Controllers
{
    /// <summary>
    /// Gestiona el catálogo de libros, incluyendo visualización premium y
    /// gestión de múltiples archivos digitales por título.
    /// </summary>
    public class LibrosController : Controller
    {
        private readonly BibliotecaContext _context;
        private readonly IWebHostEnvironment _env;

        public LibrosController(BibliotecaContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        /// <summary>
        /// Muestra el catálogo de libros con soporte para búsqueda dinámica y marcado de favoritos.
        /// </summary>
        [Authorize]
        public async Task<IActionResult> Index(string? query, int page = 1)
        {
            int pageSize = 8;
            
            var librosQuery = _context.Libros
                .Include(l => l.Autor)
                .Include(l => l.Categorias)
                .Include(l => l.Archivos)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var lowerQuery = query.ToLower();
                librosQuery = librosQuery.Where(l => 
                    l.Titulo.ToLower().Contains(lowerQuery) || 
                    l.Autor.Nombre.ToLower().Contains(lowerQuery) ||
                    l.Categorias.Any(c => c.Nombre.ToLower().Contains(lowerQuery)) ||
                    l.ISBN.Contains(lowerQuery)
                );
            }

            int totalLibros = await librosQuery.CountAsync();
            var libros = await librosQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var libroIds = libros.Select(l => l.Id).ToList();
            var ratings = await _context.Resenas
                .Where(r => libroIds.Contains(r.LibroId))
                .GroupBy(r => r.LibroId)
                .Select(g => new { LibroId = g.Key, Avg = g.Average(r => r.Puntuacion) })
                .ToDictionaryAsync(x => x.LibroId, x => x.Avg);

            foreach (var l in libros)
            {
                l.RatingCalculadoEager = ratings.ContainsKey(l.Id) ? Math.Round(ratings[l.Id], 1) : 0;
            }

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalLibros / (double)pageSize);
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

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_LibrosGrid", libros);
            }

            return View(libros);
        }

        /// <summary>
        /// Muestra la información técnica completa y la sección de archivos para descarga.
        /// </summary>
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var libro = await _context.Libros
                .Include(l => l.Autor)
                .Include(l => l.Categorias)
                .Include(l => l.Resenas).ThenInclude(r => r.Usuario)
                .Include(l => l.Archivos)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (libro == null) return NotFound();

            var recomendados = await _context.Libros
                .Include(l => l.Autor)
                .Where(l => l.Id != id && (l.AutorId == libro.AutorId || l.Categorias.Any(c => libro.Categorias.Select(lc => lc.Id).Contains(c.Id))))
                .OrderByDescending(l => l.Resenas.Average(r => (double?)r.Puntuacion) ?? 0)
                .Take(8)
                .ToListAsync();

            ViewBag.Recomendados = recomendados;
            return View(libro);
        }

        /// <summary>
        /// Procesa la publicación de una nueva reseña de usuario.
        /// Valida que el usuario no haya calificado el mismo libro anteriormente.
        /// </summary>
        /// <param name="LibroId">ID del libro a calificar.</param>
        /// <param name="Puntuacion">Valor numérico de 1 a 5.</param>
        /// <param name="Comentario">Texto opcional de la reseña.</param>
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

            var resena = new Resena { LibroId = LibroId, UsuarioId = usuarioId, Puntuacion = Puntuacion, Comentario = Comentario, FechaPublicacion = DateTime.Now };
            if (ModelState.IsValid)
            {
                _context.Resenas.Add(resena);
                await _context.SaveChangesAsync();
                TempData["Success"] = "¡Reseña publicada!";
            }
            return RedirectToAction(nameof(Details), new { id = LibroId });
        }

        /// <summary>
        /// Muestra el formulario de creación para nuevos libros.
        /// Prepara las listas desplegables de autores y categorías.
        /// </summary>
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewBag.Autores = new SelectList(_context.Autores, "Id", "Nombre");
            ViewBag.Categorias = new MultiSelectList(_context.Categorias, "Id", "Nombre");
            return View();
        }

        /// <summary>
        /// Procesa la creación de un libro y su carga inicial de archivos digitales.
        /// Los archivos se almacenan de forma segura en el Vault con nombres únicos (GUID).
        /// </summary>
        /// <param name="libro">Entidad básica del libro.</param>
        /// <param name="CategoriasSeleccionadas">IDs de las categorías a asociar.</param>
        /// <param name="archivosLibro">Colección de archivos físicos subidos desde el formulario.</param>
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

            if (await _context.Libros.AnyAsync(l => l.Titulo.ToLower() == libro.Titulo.ToLower()))
            {
                ModelState.AddModelError("Titulo", "Ya existe un libro con este título.");
                ViewBag.Autores = new SelectList(_context.Autores, "Id", "Nombre", libro.AutorId);
                ViewBag.Categorias = new MultiSelectList(_context.Categorias, "Id", "Nombre", CategoriasSeleccionadas);
                return View(libro);
            }

            if (CategoriasSeleccionadas != null)
            {
                foreach (var id in CategoriasSeleccionadas)
                {
                    var cat = await _context.Categorias.FindAsync(id);
                    if (cat != null) libro.Categorias.Add(cat);
                }
            }

            // Procesamiento de múltiples archivos
            if (archivosLibro != null && archivosLibro.Count > 0)
            {
                var vaultFolder = Path.Combine(_env.ContentRootPath, "BibliotecaLibros_Vault");
                if (!Directory.Exists(vaultFolder)) Directory.CreateDirectory(vaultFolder);

                foreach (var file in archivosLibro)
                {
                    var extension = Path.GetExtension(file.FileName).ToLower();
                    var uniqueName = Guid.NewGuid().ToString() + extension;
                    var filePath = Path.Combine(vaultFolder, uniqueName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    libro.Archivos.Add(new LibroArchivo { Ruta = uniqueName, Formato = extension });
                }
            }

            _context.Libros.Add(libro);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Muestra el formulario para editar datos de un libro y gestionar sus archivos.
        /// </summary>
        /// <param name="id">ID del libro a editar.</param>
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
        /// Procesa la actualización de un libro y la adición de nuevos archivos digitales.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Titulo,AutorId,ISBN,ImagenUrl,Descripcion")] Libro libro, int[] CategoriasSeleccionadas, IFormFileCollection nuevosArchivos)
        {
            if (id != libro.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var libroToUpdate = await _context.Libros.Include(l => l.Categorias).Include(l => l.Archivos).FirstOrDefaultAsync(l => l.Id == id);
                if (libroToUpdate == null) return NotFound();

                libroToUpdate.Titulo = libro.Titulo;
                libroToUpdate.AutorId = libro.AutorId;
                libroToUpdate.ISBN = libro.ISBN;
                libroToUpdate.ImagenUrl = libro.ImagenUrl;
                libroToUpdate.Descripcion = libro.Descripcion;

                libroToUpdate.Categorias.Clear();
                if (CategoriasSeleccionadas != null)
                {
                    foreach (var catId in CategoriasSeleccionadas)
                    {
                        var cat = await _context.Categorias.FindAsync(catId);
                        if (cat != null) libroToUpdate.Categorias.Add(cat);
                    }
                }

                if (nuevosArchivos != null && nuevosArchivos.Count > 0)
                {
                    var vaultFolder = Path.Combine(_env.ContentRootPath, "BibliotecaLibros_Vault");
                    if (!Directory.Exists(vaultFolder)) Directory.CreateDirectory(vaultFolder);

                    foreach (var file in nuevosArchivos)
                    {
                        var extension = Path.GetExtension(file.FileName).ToLower();
                        var uniqueName = Guid.NewGuid().ToString() + extension;
                        var filePath = Path.Combine(vaultFolder, uniqueName);

                        using (var stream = new FileStream(filePath, FileMode.Create)) { await file.CopyToAsync(stream); }
                        libroToUpdate.Archivos.Add(new LibroArchivo { Ruta = uniqueName, Formato = extension });
                    }
                }

                _context.Update(libroToUpdate);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Autores = new SelectList(_context.Autores, "Id", "Nombre", libro.AutorId);
            ViewBag.Categorias = new MultiSelectList(_context.Categorias, "Id", "Nombre", CategoriasSeleccionadas);
            return View(libro);
        }

        /// <summary>
        /// Muestra la confirmación de eliminación de un libro.
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
        /// Realiza la eliminación física de un libro y todos sus archivos digitales en el Vault.
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var libro = await _context.Libros.Include(l => l.Archivos).FirstOrDefaultAsync(l => l.Id == id);
            if (libro != null)
            {
                var vaultFolder = Path.Combine(_env.ContentRootPath, "BibliotecaLibros_Vault");
                foreach (var archivo in libro.Archivos)
                {
                    var filePath = Path.Combine(vaultFolder, archivo.Ruta);
                    if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                }
                _context.Libros.Remove(libro);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Libro y archivos eliminados.";
            }
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Acceso directo a la pasarela de préstamo desde el catálogo detallado.
        /// </summary>
        [Authorize]
        public IActionResult Prestar(int id)
        {
            var libro = _context.Libros.Include(l => l.Archivos).FirstOrDefault(l => l.Id == id);
            if (libro == null) return NotFound();
            if (!libro.Archivos.Any())
            {
                TempData["Error"] = "Libro sin archivos digitales.";
                return RedirectToAction("Index", "Home");
            }
            ViewBag.LibroTitulo = libro.Titulo;
            return View(new Prestamo { LibroId = id });
        }
        
        /// <summary>
        /// Valida la existencia de un libro por ID.
        /// </summary>
        private bool LibroExists(int id) => _context.Libros.Any(e => e.Id == id);
    }
}
