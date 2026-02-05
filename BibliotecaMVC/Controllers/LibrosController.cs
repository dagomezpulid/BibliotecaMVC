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
        // GET: Libros
        public IActionResult Index()
        {
            var libros = _context.Libros
                .Include(l => l.Autor)
                .ToList();

            return View(libros);
        }
        [Authorize(Roles = "Admin")]
        // GET: Libros/Create
        public IActionResult Create()
        {
            ViewBag.Autores = new SelectList(_context.Autores, "AutorID", "Nombre");
            return View();
        }

        // POST: Libros/Create
        [Authorize(Roles = "Admin")]
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
            return View(new Prestamo { LibroID = id });
        }
    }
}

