using BibliotecaMVC.Models;
using BibliotecaMVC.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Panel de control administrativo para la gestión global del sistema.
/// Permite supervisar usuarios, escalar privilegios, rehabilitar cuentas con mora
/// y realizar bajas controladas con eliminación en cascada.
/// </summary>
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly BibliotecaContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration; // Almacena la configuración (User Secrets/appsettings)

    public AdminController(BibliotecaContext context, UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        _context = context;
        _userManager = userManager;
        _configuration = configuration;
    }
    /// <summary>
    /// Genera la vista principal del Dashboard con estadísticas consolidadas.
    /// Resuelve problemas de N+1 al cargar préstamos y multas de forma optimizada.
    /// </summary>
    /// <returns>Modelo de vista con contadores globales y lista de usuarios.</returns>
    public async Task<IActionResult> Index()
    {
        // 1. Cargar usuarios
        var users = await _userManager.Users.ToListAsync();
        var usuariosIds = users.Select(u => u.Id).ToList();

        // N+1 SOLVED: Cargar todos los préstamos activos globalmente
        var prestamosActivosAll = await _context.Prestamos
            .Where(p => p.FechaDevolucionReal == null && p.UsuarioId != null && usuariosIds.Contains(p.UsuarioId))
            .ToListAsync();

        // 2. Preparar el listado visual
        var userViewModels = new List<UserViewModel>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            userViewModels.Add(new UserViewModel
            {
                Id = user.Id,
                NombreCompleto = user.NombreCompleto,
                Email = user.Email ?? "Sin Email",
                Roles = roles.ToList(),
                EstaBloqueado = user.BloqueadoParaPrestamos
            });
        }

        // --- CÁLCULO DE DATOS PARA GRÁFICOS ---

        // A. Usuarios con más mora histórica (Top 5)
        var morososData = await _context.Multas
            .Include(m => m.Prestamo)
                .ThenInclude(p => p!.Usuario)
            .GroupBy(m => new { m.Prestamo!.UsuarioId, m.Prestamo.Usuario!.Nombre, m.Prestamo.Usuario!.Apellido })
            .Select(g => new {
                Nombre = g.Key.Nombre + " " + g.Key.Apellido,
                TotalMora = g.Sum(m => m.Monto),
                // Calculamos los días de mora totales sumando la diferencia de días entre lo programado y lo real (o ahora)
                TotalDias = g.Sum(m => EF.Functions.DateDiffDay(m.Prestamo!.FechaDevolucionProgramada, m.Prestamo.FechaDevolucionReal ?? DateTime.Now))
            })
            .OrderByDescending(x => x.TotalMora)
            .Take(5)
            .ToListAsync();

        // B. Libros más prestados (Top 5)
        var librosPopularesData = await _context.Prestamos
            .Include(p => p.Libro)
            .GroupBy(p => p.Libro!.Titulo)
            .Select(g => new {
                Titulo = g.Key,
                Cantidad = g.Count()
            })
            .OrderByDescending(x => x.Cantidad)
            .Take(5)
            .ToListAsync();

        // C. Tendencia de préstamos (Últimos 6 meses)
        var seisMesesAtras = DateTime.Now.AddMonths(-5);
        var tendenciaData = await _context.Prestamos
            .Where(p => p.FechaPrestamo >= new DateTime(seisMesesAtras.Year, seisMesesAtras.Month, 1))
            .GroupBy(p => new { p.FechaPrestamo.Year, p.FechaPrestamo.Month })
            .Select(g => new {
                Anio = g.Key.Year,
                Mes = g.Key.Month,
                Cantidad = g.Count()
            })
            .OrderBy(x => x.Anio).ThenBy(x => x.Mes)
            .ToListAsync();

        // 3. Preparar el modelo general del Dashboard
        var model = new AdminDashboardViewModel
        {
            TotalUsuarios = users.Count,
            TotalLibros = await _context.Libros.CountAsync(),
            TotalAutores = await _context.Autores.CountAsync(),
            PrestamosActivos = prestamosActivosAll.Count,
            TotalMultasPendientes = await _context.Multas
                        .Where(m => !m.Pagada)
                        .SumAsync(m => (decimal?)m.Monto) ?? 0,
            Usuarios = userViewModels,

            // Poblado de analíticas
            LabelsMorosos = morososData.Select(x => x.Nombre).ToList(),
            ValoresMorosos = morososData.Select(x => x.TotalMora).ToList(),
            ValoresDiasMora = morososData.Select(x => x.TotalDias).ToList(),

            LabelsLibrosPopulares = librosPopularesData.Select(x => x.Titulo ?? "Sin Título").ToList(),
            ValoresLibrosPopulares = librosPopularesData.Select(x => x.Cantidad).ToList(),

            LabelsTendencia = tendenciaData.Select(x => new DateTime(x.Anio, x.Mes, 1).ToString("MMM yyyy")).ToList(),
            ValoresTendencia = tendenciaData.Select(x => x.Cantidad).ToList()
        };

        return View(model);
    }

    /// <summary>
    /// Eleva a un usuario al rol de Administrador.
    /// </summary>
    /// <param name="id">ID de Identity del usuario.</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> HacerAdmin(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        if (!await _userManager.IsInRoleAsync(user, "Admin"))
        {
            await _userManager.AddToRoleAsync(user, "Admin");
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Revoca el rol de Administrador a un usuario existente.
    /// </summary>
    /// <param name="id">ID de Identity del usuario al que se le retira el privilegio.</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuitarAdmin(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            await _userManager.RemoveFromRoleAsync(user, "Admin");
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Restablece los privilegios de préstamo de un usuario que estaba suspendido por mora.
    /// Conocido técnicamente como el proceso de "Amnistía Administrativa".
    /// </summary>
    /// <param name="id">ID del usuario a rehabilitar.</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DesbloquearUsuario(string id)
    {
        var usuario = await _userManager.FindByIdAsync(id);
        if (usuario == null) return NotFound();

        usuario.BloqueadoParaPrestamos = false;
        await _userManager.UpdateAsync(usuario);

        TempData["Success"] = $"El usuario {usuario.NombreCompleto} ha sido rehabilitado exitosamente para nuevos préstamos.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Elimina a un usuario del sistema de forma segura.
    /// Realiza una limpieza recursiva de Pagos, Multas e Historial de Préstamos
    /// para evitar errores de integridad referencial.
    /// </summary>
    /// <param name="id">ID del usuario a eliminar.</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarUsuario(string id)
    {
        var usuario = await _userManager.FindByIdAsync(id);

        if (usuario == null)
            return NotFound();

        if (usuario.Id == _userManager.GetUserId(User))
        {
            TempData["Error"] = "No puedes eliminar tu propio usuario.";
            return RedirectToAction(nameof(Index));
        }

        // Protección dinámica: Evita eliminar al administrador principal configurado en appsettings/Secrets
        // Esto previene que se bloquee el acceso raíz al sistema.
        string masterAdminEmail = _configuration["AdminSettings:Email"] ?? "dgomezpulid@outlook.com";
        if (usuario.Email != null && usuario.Email.Equals(masterAdminEmail, StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "No se puede eliminar el administrador principal configurado en el sistema.";
            return RedirectToAction(nameof(Index));
        }

        var tienePrestamosActivos = _context.Prestamos.Any(p => p.UsuarioId == usuario.Id && p.FechaDevolucionReal == null);

        if (tienePrestamosActivos)
        {
            TempData["Error"] = "No se puede eliminar un usuario con préstamos activos.";
            return RedirectToAction(nameof(Index));
        }

        // MIGRACIÓN A USUARIO FANTASMA PARA PROTEGER ANALÍTICAS
        // En lugar de borrar préstamos y reseñas, las asignamos a un perfil anónimo 
        // conservando las estadísticas mensuales y de popularidad.
        var ghostUser = await _userManager.FindByEmailAsync("fantasma@biblioteca.net");
        if (ghostUser == null)
        {
            ghostUser = new ApplicationUser
            {
                UserName = "fantasma@biblioteca.net",
                Email = "fantasma@biblioteca.net",
                Nombre = "Usuario",
                Apellido = "Eliminado",
                EmailConfirmed = true
            };
            await _userManager.CreateAsync(ghostUser, "G0st!_B0y99z");
        }

        var historialPrestamos = await _context.Prestamos.Where(p => p.UsuarioId == usuario.Id).ToListAsync();
        foreach (var p in historialPrestamos) p.UsuarioId = ghostUser.Id;

        // Limpieza de datos estrictamente personales (Favoritos y Alertas internas)
        var favoritos = await _context.Favoritos.Where(f => f.UsuarioId == usuario.Id).ToListAsync();
        var notificaciones = await _context.Notificaciones.Where(n => n.UsuarioId == usuario.Id).ToListAsync();
        var resenas = await _context.Resenas.Where(r => r.UsuarioId == usuario.Id).ToListAsync();
        foreach (var r in resenas) r.UsuarioId = ghostUser.Id;

        _context.Favoritos.RemoveRange(favoritos);
        _context.Notificaciones.RemoveRange(notificaciones);

        await _context.SaveChangesAsync();

        await _userManager.DeleteAsync(usuario);

        TempData["Success"] = "Usuario y su historial han sido eliminados correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // Métodos aislados removidos para delegar al Modelo (Clean Code)
}