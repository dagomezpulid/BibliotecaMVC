using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BibliotecaMVC.Models;

namespace BibliotecaMVC.Controllers
{
    public class LibrosController : Controller
    {
        private readonly BibliotecaContext _context;

        public LibrosController(BibliotecaContext context)
        {
            _context = context;
        }

        // GET: Libros
        public IActionResult Index()
        {
            var libros = _context.Libros
                .Include(l => l.Autor)
                .ToList();

            return View(libros);
        }

        // GET: Libros/Create
        public IActionResult Create()
        {
            ViewBag.Autores = new SelectList(_context.Autores, "AutorID", "Nombre");
            return View();
        }

        // POST: Libros/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Libro libro)
        {

            if (!ModelState.IsValid)
            {

                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage);
                }

                ViewBag.Autores = new SelectList(_context.Autores, "AutorID", "Nombre", libro.AutorID);
                return View(libro);
            }

            _context.Libros.Add(libro);
            _context.SaveChanges();

            return RedirectToAction("Index", "Home");
        }

        // GET Libros/Prestamo
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
            return View(new Prestamo { LibroID = id });
        }

        // POST Libros/Prestamo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Prestar(Prestamo prestamo)
        {
            var libro = _context.Libros.Find(prestamo.LibroID);

            if (libro == null)
                return NotFound();

            if (libro.Stock <= 0)
            {
                ModelState.AddModelError("", "No hay stock disponible.");
                ViewBag.LibroTitulo = libro.Titulo;
                return View(prestamo);
            }

            libro.Stock -= 1;
            prestamo.FechaPrestamo = DateTime.Now;

            _context.Prestamos.Add(prestamo);
            _context.Libros.Update(libro);
            _context.SaveChanges();

            return RedirectToAction("Index", "Home");
        }
    }
}

