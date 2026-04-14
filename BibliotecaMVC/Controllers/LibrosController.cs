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
    /// Gestiona el catálogo de libros, incluyendo visualización premium, 
    /// creación con metadatos (ISBN, Portadas) y edición de categorías.
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
        /// <param name="query">Término de búsqueda opcional (Título, Autor, Categoría o ISBN).</param>
        /// <returns>Vista con listado de libros filtrados.</returns>
        [Authorize]
        public async Task<IActionResult> Index(string? query, int page = 1)
        {
            int pageSize = 8; // Cantidad de libros por página (Escalabilidad)
            
            var librosQuery = _context.Libros
                .Include(l => l.Autor)
                .Include(l => l.Categorias)
                .Include(l => l.Resenas)
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

            // Paginación segmentada para evitar cargar miles de registros en memoria
            int totalLibros = await librosQuery.CountAsync();
            var libros = await librosQuery
                .Skip((page - 1) * pageSize) // Saltear registros de páginas anteriores
                .Take(pageSize)              // Tomar solo la "rebanada" necesaria
                .ToListAsync();

            // Metadatos para la navegación en la vista
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalLibros / (double)pageSize);
            ViewBag.Query = query;

            // Lógica de Favoritos: Marcar los libros que ya tiene el usuario
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
        /// Muestra la información técnica completa y la sección de reseñas del libro.
        /// </summary>
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var libro = await _context.Libros
                .Include(l => l.Autor)
                .Include(l => l.Categorias)
                .Include(l => l.Resenas)
                    .ThenInclude(r => r.Usuario)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (libro == null) return NotFound();

            // --- MOTOR DE RECOMENDACIONES (Fase 4) ---
            var categoriaIds = libro.Categorias.Select(c => c.Id).ToList();
            
            // 1. Recomendaciones por Categoría (50%)
            var recomendadosPorCategoria = await _context.Libros
                .Include(l => l.Autor)
                .Where(l => l.Id != id && l.Categorias.Any(c => categoriaIds.Contains(c.Id)))
                .OrderByDescending(l => l.Resenas.Average(r => (double?)r.Puntuacion) ?? 0)
                .Take(4)
                .ToListAsync();

            // 2. Recomendaciones por Autor (50%)
            var recomendadosPorAutor = await _context.Libros
                .Include(l => l.Autor)
                .Where(l => l.Id != id && l.AutorId == libro.AutorId)
                .OrderByDescending(l => l.Resenas.Average(r => (double?)r.Puntuacion) ?? 0)
                .Take(4)
                .ToListAsync();

            // 3. Combinar y limpiar duplicados
            var recomendadosDocs = recomendadosPorCategoria
                .Union(recomendadosPorAutor)
                .DistinctBy(l => l.Id)
                .Take(8)
                .ToList();

            ViewBag.Recomendados = recomendadosDocs;

            return View(libro);
        }

        /// <summary>
        /// Registra una nueva reseña y calificación en el sistema.
        /// </summary>
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostResena(int LibroId, int Puntuacion, string Comentario)
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(usuarioId)) return Unauthorized();

            // Validar si el usuario ya dejó una reseña para este libro (Antispam)
            var yaReseno = await _context.Resenas.AnyAsync(r => r.LibroId == LibroId && r.UsuarioId == usuarioId);
            if (yaReseno)
            {
                TempData["Error"] = "Ya has calificado este libro anteriormente.";
                return RedirectToAction(nameof(Details), new { id = LibroId });
            }

            var resena = new Resena
            {
                LibroId = LibroId,
                UsuarioId = usuarioId,
                Puntuacion = Puntuacion,
                Comentario = Comentario,
                FechaPublicacion = DateTime.Now
            };

            if (ModelState.IsValid)
            {
                _context.Resenas.Add(resena);
                await _context.SaveChangesAsync();
                TempData["Success"] = "¡Gracias por tu opinión! Tu reseña ha sido publicada.";
            }
            else
            {
                TempData["Error"] = "Hubo un error al procesar tu reseña. Por favor intenta de nuevo.";
            }

            return RedirectToAction(nameof(Details), new { id = LibroId });
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewBag.Autores = new SelectList(_context.Autores, "Id", "Nombre");
            ViewBag.Categorias = new MultiSelectList(_context.Categorias, "Id", "Nombre");
            return View();
        }

        /// <summary>
        /// Procesa la creación de un nuevo libro.
        /// Realiza validaciones de duplicidad por título y asocia metadatos extendidos.
        /// </summary>
        /// <param name="libro">Modelo del libro a crear (ISBN, ImagenUrl, etc).</param>
        /// <param name="CategoriasSeleccionadas">Array de IDs de las categorías seleccionadas.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Titulo,AutorId,Stock,ISBN,ImagenUrl,Descripcion")] Libro libro, int[] CategoriasSeleccionadas, IFormFile? archivoLibro)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Autores = new SelectList(_context.Autores, "Id", "Nombre", libro.AutorId);
                ViewBag.Categorias = new MultiSelectList(_context.Categorias, "Id", "Nombre", CategoriasSeleccionadas);
                return View(libro);
            }

            bool existeLibro = await _context.Libros.AnyAsync(l => 
                l.Titulo.ToLower() == libro.Titulo.ToLower());

            if (existeLibro)
            {
                ModelState.AddModelError("Titulo", "Ya existe un libro registrado con este mismo título.");
                ViewBag.Autores = new SelectList(_context.Autores, "Id", "Nombre", libro.AutorId);
                ViewBag.Categorias = new MultiSelectList(_context.Categorias, "Id", "Nombre", CategoriasSeleccionadas);
                return View(libro);
            }

            if (CategoriasSeleccionadas != null)
            {
                foreach (var id in CategoriasSeleccionadas)
                {
                    var categoria = await _context.Categorias.FindAsync(id);
                    if (categoria != null) libro.Categorias.Add(categoria);
                }
            }

            // Procesamiento de Almacenamiento Digital
            if (archivoLibro != null && archivoLibro.Length > 0)
            {
                var allowedExtensions = new[] { ".pdf", ".epub", ".doc", ".docx" };
                var extension = Path.GetExtension(archivoLibro.FileName).ToLower();
                
                if (allowedExtensions.Contains(extension))
                {
                    var uploadsFolder = Path.Combine(_env.WebRootPath, "archivos_libros");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(archivoLibro.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await archivoLibro.CopyToAsync(stream);
                    }
                    
                    libro.ArchivoRuta = "/archivos_libros/" + uniqueFileName;
                }
                else
                {
                    ModelState.AddModelError("ArchivoRuta", "Formato no válido. Solo se permiten archivos PDF, EPUB, DOC o DOCX.");
                    ViewBag.Autores = new SelectList(_context.Autores, "Id", "Nombre", libro.AutorId);
                    ViewBag.Categorias = new MultiSelectList(_context.Categorias, "Id", "Nombre", CategoriasSeleccionadas);
                    return View(libro);
                }
            }

            _context.Libros.Add(libro);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var libro = await _context.Libros
                .Include(l => l.Categorias)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (libro == null) return NotFound();

            ViewBag.Autores = new SelectList(_context.Autores, "Id", "Nombre", libro.AutorId);
            ViewBag.Categorias = new MultiSelectList(_context.Categorias, "Id", "Nombre", libro.Categorias.Select(c => c.Id));
            
            return View(libro);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Titulo,AutorId,Stock,ISBN,ImagenUrl,Descripcion,RowVersion")] Libro libro, int[] CategoriasSeleccionadas, IFormFile? archivoLibro)
        {
            if (id != libro.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var libroToUpdate = await _context.Libros
                        .Include(l => l.Categorias)
                        .FirstOrDefaultAsync(l => l.Id == id);

                    if (libroToUpdate == null) return NotFound();

                    libroToUpdate.Titulo = libro.Titulo;
                    libroToUpdate.AutorId = libro.AutorId;
                    libroToUpdate.Stock = libro.Stock;
                    libroToUpdate.ISBN = libro.ISBN;
                    libroToUpdate.ImagenUrl = libro.ImagenUrl;
                    libroToUpdate.Descripcion = libro.Descripcion;
                    
                    // Se asigna el RowVersion original enviado desde la vista. 
                    // Si el valor en DB cambió mientras el usuario editaba, EF lanzará una DbUpdateConcurrencyException.
                    libroToUpdate.RowVersion = libro.RowVersion;

                    // Actualizar categorías
                    libroToUpdate.Categorias.Clear();
                    if (CategoriasSeleccionadas != null)
                    {
                        foreach (var catId in CategoriasSeleccionadas)
                        {
                            var cat = await _context.Categorias.FindAsync(catId);
                            if (cat != null) libroToUpdate.Categorias.Add(cat);
                        }
                    }

                    // Procesamiento de Almacenamiento Digital
                    if (archivoLibro != null && archivoLibro.Length > 0)
                    {
                        var allowedExtensions = new[] { ".pdf", ".epub", ".doc", ".docx" };
                        var extension = Path.GetExtension(archivoLibro.FileName).ToLower();
                        
                        if (allowedExtensions.Contains(extension))
                        {
                            var uploadsFolder = Path.Combine(_env.WebRootPath, "archivos_libros");
                            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                            
                            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(archivoLibro.FileName);
                            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                            
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await archivoLibro.CopyToAsync(stream);
                            }
                            
                            // Reemplazar la ruta antigua por la nueva (en DB)
                            libroToUpdate.ArchivoRuta = "/archivos_libros/" + uniqueFileName;
                        }
                        else
                        {
                            ModelState.AddModelError("ArchivoRuta", "Formato no válido. Solo se permiten archivos PDF, EPUB, DOC o DOCX.");
                            ViewBag.Autores = new SelectList(_context.Autores, "Id", "Nombre", libro.AutorId);
                            ViewBag.Categorias = new MultiSelectList(_context.Categorias, "Id", "Nombre", CategoriasSeleccionadas);
                            return View(libro);
                        }
                    }

                    _context.Update(libroToUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LibroExists(libro.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Autores = new SelectList(_context.Autores, "Id", "Nombre", libro.AutorId);
            ViewBag.Categorias = new MultiSelectList(_context.Categorias, "Id", "Nombre", CategoriasSeleccionadas);
            return View(libro);
        }

        private bool LibroExists(int id)
        {
            return _context.Libros.Any(e => e.Id == id);
        }

        [Authorize]
        public IActionResult Prestar(int id)
        {
            var libro = _context.Libros.Find(id);

            if (libro == null)
                return NotFound();

            if (libro.Stock <= 0)
            {
                TempData["Error"] = "No hay stock disponible para este libro.";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.LibroTitulo = libro.Titulo;

            return View(new Prestamo
            {
                LibroId = id
            });
        }
    }
}
