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

        public LibrosController(BibliotecaContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Muestra el catálogo completo de libros. Soporta filtrado por texto (Título, Autor, Categoría).
        /// </summary>
        /// <param name="query">Palabra clave de búsqueda.</param>
        [Authorize]
        public async Task<IActionResult> Index(string query)
        {
            var librosQuery = _context.Libros
                .Include(l => l.Autor)
                .Include(l => l.Categorias)
                .Include(l => l.Resenas)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.ToLower();
                librosQuery = librosQuery.Where(l => 
                    l.Titulo.ToLower().Contains(query) || 
                    l.Autor.Nombre.ToLower().Contains(query) ||
                    l.Categorias.Any(c => c.Nombre.ToLower().Contains(query))
                );
            }

            var libros = await librosQuery.ToListAsync();

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
        public async Task<IActionResult> Create([Bind("Titulo,AutorId,Stock,ISBN,ImagenUrl,Descripcion")] Libro libro, int[] CategoriasSeleccionadas)
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,Titulo,AutorId,Stock,ISBN,ImagenUrl,Descripcion")] Libro libro, int[] CategoriasSeleccionadas)
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
