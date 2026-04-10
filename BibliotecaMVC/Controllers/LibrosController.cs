using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

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
        /// Muestra el catálogo completo de libros en formato de tarjetas premium.
        /// </summary>
        /// <returns>Vista con la lista de libros, autores y sus categorías asociadas.</returns>
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var libros = await _context.Libros
                .Include(l => l.Autor)
                .Include(l => l.Categorias)
                .ToListAsync();

            return View(libros);
        }

        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var libro = await _context.Libros
                .Include(l => l.Autor)
                .Include(l => l.Categorias)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (libro == null) return NotFound();

            return View(libro);
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
