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

        /// <summary>
        /// Muestra el formulario para registrar un nuevo autor.
        /// </summary>
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

            return RedirectToAction("Index");
        }

        // --- MÉTODOS CRUD FALTANTES AÑADIDOS --- //

        /// <summary>
        /// Muestra el formulario para editar un autor existente.
        /// </summary>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var autor = await _context.Autores.FindAsync(id);
            if (autor == null) return NotFound();
            return View(autor);
        }

        /// <summary>
        /// Procesa la modificación de datos de un autor.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre")] Autor autor)
        {
            if (id != autor.Id) return NotFound();
            if (!ModelState.IsValid) return View(autor);

            bool existeDuplicado = await _context.Autores.AnyAsync(a => 
                a.Id != autor.Id && a.Nombre.ToLower() == autor.Nombre.ToLower());
            if (existeDuplicado)
            {
                ModelState.AddModelError("Nombre", "El nombre ingresado pertenece a otro autor.");
                return View(autor);
            }

            try
            {
                _context.Update(autor);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Autor modificado con éxito.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Autores.Any(e => e.Id == autor.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Muestra la vista de confirmación para eliminar un autor.
        /// </summary>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var autor = await _context.Autores.FirstOrDefaultAsync(m => m.Id == id);
            if (autor == null) return NotFound();
            return View(autor);
        }

        /// <summary>
        /// Procesa la baja de un autor. Verifica que no tenga libros para mantener la integridad.
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var autor = await _context.Autores.FindAsync(id);
            if (autor != null)
            {
                // Validación de integridad relacional
                var tieneLibros = await _context.Libros.AnyAsync(l => l.AutorId == autor.Id);
                if (tieneLibros)
                {
                    TempData["Error"] = "Para borrar al autor, primero debe eliminar todos sus libros o reasignarlos.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Autores.Remove(autor);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Autor eliminado correctamente.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

