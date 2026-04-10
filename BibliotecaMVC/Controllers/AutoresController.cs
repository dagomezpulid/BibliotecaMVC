using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaMVC.Controllers
{
    /// <summary>
    /// Gestiona el registro y mantenimiento de autores literarios.
    /// Acceso restringido únicamente a usuarios con rol de Administrador.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AutoresController : Controller
    {
        private readonly BibliotecaContext _context;

        public AutoresController(BibliotecaContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lista todos los autores registrados en el sistema.
        /// </summary>
        /// <returns>Vista con la colección de autores.</returns>
        public IActionResult Index()
        {
            var autores = _context.Autores.ToList();
            return View(autores);
        }

        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Procesa la creación de un nuevo autor.
        /// Valida la integridad de los datos y previene la duplicidad de nombres mediante AnyAsync.
        /// </summary>
        /// <param name="autor">Entidad Autor a persistir.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre")] Autor autor)
        {
            if (!ModelState.IsValid)
                return View(autor);

            bool existeAutor = await _context.Autores.AnyAsync(a => 
                a.Nombre.ToLower() == autor.Nombre.ToLower());

            if (existeAutor)
            {
                ModelState.AddModelError("Nombre", "Ya existe un autor registrado con ese mismo nombre.");
                return View(autor);
            }

            _context.Autores.Add(autor);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}

