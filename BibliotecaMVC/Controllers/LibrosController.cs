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
    }
}

