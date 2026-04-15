using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BibliotecaMVC.Models;

namespace BibliotecaMVC.Controllers
{
    /// <summary>
    /// Gestiona la clasificación temática de los libros en el catálogo.
    /// Acceso restringido a administradores para mantener la taxonomía del sistema.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class CategoriasController : Controller
    {
        private readonly BibliotecaContext _context;

        /// <summary>
        /// Inicializa el controlador con el contexto de datos.
        /// </summary>
        public CategoriasController(BibliotecaContext context)
        {
            _context = context;
        }

        /// <summary>Lista las categorías registradas.</summary>
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categorias.ToListAsync());
        }

        /// <summary>
        /// Muestra el formulario para crear una nueva categoría.
        /// </summary>
        public IActionResult Create() => View();

        /// <summary>
        /// Procesa la creación de una categoría, validando que no exista una con el mismo nombre.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre")] Categoria categoria)
        {
            if (ModelState.IsValid)
            {
                if (await _context.Categorias.AnyAsync(c => c.Nombre.ToLower() == categoria.Nombre.ToLower()))
                {
                    ModelState.AddModelError("Nombre", "La categoría ya existe.");
                    return View(categoria);
                }
                
                _context.Add(categoria);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(categoria);
        }

        /// <summary>
        /// Muestra el formulario para editar una categoría existente.
        /// </summary>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null) return NotFound();
            return View(categoria);
        }

        /// <summary>
        /// Procesa la edición de una categoría con validación de duplicados.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre")] Categoria categoria)
        {
            if (id != categoria.Id) return NotFound();

            if (ModelState.IsValid)
            {
                if (await _context.Categorias.AnyAsync(c => c.Id != categoria.Id && c.Nombre.ToLower() == categoria.Nombre.ToLower()))
                {
                    ModelState.AddModelError("Nombre", "El nombre de categoría ya existe.");
                    return View(categoria);
                }

                try
                {
                    _context.Update(categoria);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Categorias.Any(e => e.Id == categoria.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(categoria);
        }

        /// <summary>
        /// Muestra el formulario de confirmación para eliminar una categoría.
        /// </summary>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var categoria = await _context.Categorias.FirstOrDefaultAsync(m => m.Id == id);
            if (categoria == null) return NotFound();
            return View(categoria);
        }

        /// <summary>
        /// Procesa la eliminación de una categoría. 
        /// Valida la integridad referencial para no borrar categorías que aún tengan libros asociados.
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var categoria = await _context.Categorias.Include(c => c.Libros).FirstOrDefaultAsync(c => c.Id == id);
            if (categoria != null)
            {
                if (categoria.Libros.Any())
                {
                    TempData["Error"] = "Para borrar esta categoría, primero debe desasociar todos los libros que la contienen.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Categorias.Remove(categoria);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Categoría eliminada con éxito.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
