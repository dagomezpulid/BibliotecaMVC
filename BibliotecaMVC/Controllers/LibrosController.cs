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
        public IActionResult Create(Libro libro)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Autores = new SelectList(
                    _context.Autores,
                    "Id",
                    "Nombre",
                    libro.AutorId);

                return View(libro);
            }

            _context.Libros.Add(libro);
            _context.SaveChanges();

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
