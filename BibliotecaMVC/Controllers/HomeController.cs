using System.Diagnostics;
using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace BibliotecaMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly BibliotecaContext _context;

        public HomeController(BibliotecaContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var libros = _context.Libros
                .Include(l => l.Autor)
                .ToList();

            return View(libros);
        }
    }
}
