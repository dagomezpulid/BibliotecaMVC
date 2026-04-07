using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaMVC.Controllers
{
    public class LibrosController : Controller
    {
        private readonly BibliotecaContext _context;

        public LibrosController(BibliotecaContext context)
        {
            _context = context;
        }

        [Authorize]
        public IActionResult Index()
        {
            var libros = _context.Libros
                .Include(l => l.Autor)
                .ToList();

            return View(libros);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewBag.Autores = new SelectList(_context.Autores, "Id", "Nombre");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Titulo,AutorId,Stock")] Libro libro)
        {
            // 1. Validar que no manden el formulario vacío
            if (!ModelState.IsValid)
            {
                ViewBag.Autores = new SelectList(
                    _context.Autores,
                    "Id",
                    "Nombre",
                    libro.AutorId);

                return View(libro);
            }

            // 2. Solo si el título tiene texto legítimo, buscamos clones
            bool existeLibro = await _context.Libros.AnyAsync(l => 
                l.Titulo.ToLower() == libro.Titulo.ToLower());

            if (existeLibro)
            {
                ModelState.AddModelError("Titulo", "Ya existe un libro registrado con este mismo título en la biblioteca.");
                
                ViewBag.Autores = new SelectList(
                    _context.Autores,
                    "Id",
                    "Nombre",
                    libro.AutorId);

                return View(libro);
            }

            _context.Libros.Add(libro);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
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
