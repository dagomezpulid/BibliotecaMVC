using BibliotecaMVC.Models;
using BibliotecaMVC.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BibliotecaMVC.Controllers
{
    /// <summary>
    /// Gestiona el panel de control personalizado para el lector.
    /// Proporciona una visión 360° de la actividad de lectura, deudas y recomendaciones.
    /// </summary>
    [Authorize(Roles = "Usuario")]
    public class DashboardController : Controller
    {
        private readonly BibliotecaContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(BibliotecaContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Punto de entrada del Dashboard del Usuario. 
        /// Orquesta la recolección de métricas y datos analíticos personales.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var userId = user.Id;

            // 1. Estadísticas Básicas
            var prestamosActivos = await _context.Prestamos
                .Where(p => p.UsuarioId == userId && p.FechaDevolucionReal == null)
                .Include(p => p.Libro)
                .ToListAsync();

            var librosLeidos = await _context.Prestamos
                .Where(p => p.UsuarioId == userId && p.FechaDevolucionReal != null)
                .CountAsync();

            var deudaTotal = await _context.Multas
                .Where(m => m.Prestamo != null && m.Prestamo.UsuarioId == userId && !m.Pagada)
                .SumAsync(m => (decimal?)m.Monto) ?? 0;

            var favoritosCount = await _context.Favoritos
                .Where(f => f.UsuarioId == userId)
                .CountAsync();

            // 2. Lectura Actual con Progreso
            var lecturaActualDtos = new List<PrestamoProgresoDTO>();
            foreach (var p in prestamosActivos)
            {
                var progreso = await _context.ProgresosLectura
                    .FirstOrDefaultAsync(pr => pr.UsuarioId == userId && pr.LibroId == p.LibroId);

                lecturaActualDtos.Add(new PrestamoProgresoDTO
                {
                    PrestamoId = p.Id,
                    TituloLibro = p.Libro?.Titulo ?? "Libro desconocido",
                    Autor = (await _context.Autores.FindAsync(p.Libro?.AutorId))?.Nombre ?? "Autor",
                    Portada = p.Libro?.ImagenUrl ?? "",
                    PaginaActual = progreso?.PaginaActual ?? 0,
                    PaginasTotales = 350 // Mock: En producción esto vendría metadata del archivo
                });
            }

            // 3. Análisis de Categorías Preferidas (Gráfica)
            var categoriasData = await _context.Prestamos
                .Where(p => p.UsuarioId == userId)
                .SelectMany(p => p.Libro!.Categorias)
                .GroupBy(c => c.Nombre)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => new { Nombre = g.Key, Cantidad = g.Count() })
                .ToListAsync();

            // 4. Recomendaciones Inteligentes
            // Basadas en las categorías más leídas del usuario
            var topCategoriasNombres = categoriasData.Select(c => c.Nombre).ToList();
            var recomendaciones = await _context.Libros
                .Where(l => l.Categorias.Any(c => topCategoriasNombres.Contains(c.Nombre)))
                // Evitar recomendar libros que ya está leyendo
                .Where(l => !prestamosActivos.Select(p => p.LibroId).Contains(l.Id))
                .OrderBy(x => Guid.NewGuid()) // Random
                .Take(3)
                .Include(l => l.Autor)
                .ToListAsync();

            var model = new UserDashboardViewModel
            {
                NombreUsuario = user.Nombre,
                PrestamosActivos = prestamosActivos.Count,
                LibrosLeidosTotal = librosLeidos,
                DeudaTotal = deudaTotal,
                FavoritosCount = favoritosCount,
                LecturaActual = lecturaActualDtos,
                LabelsCategorias = categoriasData.Select(c => c.Nombre).ToList(),
                ValoresCategorias = categoriasData.Select(c => c.Cantidad).ToList(),
                Recomendaciones = recomendaciones
            };

            return View(model);
        }
    }
}
