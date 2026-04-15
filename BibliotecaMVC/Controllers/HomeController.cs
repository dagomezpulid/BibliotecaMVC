using System.Diagnostics;
using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace BibliotecaMVC.Controllers
{
    /// <summary>
    /// Punto de entrada público de la aplicación.
    /// Enfocado en la visualización del catálogo para visitantes y gestión de errores.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly BibliotecaContext _context;

        public HomeController(BibliotecaContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Página de bienvenida pública. Carga el catálogo completo con sus autores
        /// para que los visitantes puedan explorar los títulos disponibles.
        /// </summary>
        /// <returns>Vista con la lista de libros e información de sus autores.</returns>
        public IActionResult Index()
        {
            var libros = _context.Libros
                .Include(l => l.Autor)
                .ToList();

            return View(libros);
        }
    }
}
