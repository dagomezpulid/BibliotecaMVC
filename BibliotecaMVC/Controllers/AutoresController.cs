using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AutoresController : Controller
    {
        private readonly BibliotecaContext _context;

        public AutoresController(BibliotecaContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var autores = _context.Autores.ToList();
            return View(autores);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Autor autor)
        {
            // Excepción: Evitar Autores Duplicados
            bool existeAutor = await _context.Autores.AnyAsync(a => 
                a.Nombre.ToLower() == autor.Nombre.ToLower());

            if (existeAutor)
            {
                ModelState.AddModelError("", "Ya existe un autor registrado con ese mismo nombre.");
            }

            if (!ModelState.IsValid)
                return View(autor);

            _context.Autores.Add(autor);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}

