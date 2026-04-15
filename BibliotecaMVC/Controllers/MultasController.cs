using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CsvHelper;
using System.Globalization;
using System.IO;

/// <summary>
/// Gestiona la visualización, liquidación y auditoría de sanciones financieras.
/// Permite a los usuarios pagar multas mediante una pasarela simulada 
/// y a los administradores supervisar el estado de las deudas globales.
/// </summary>
[Authorize]
public class MultasController : Controller
{
    private readonly BibliotecaContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>
    /// Crea una notificación persistente en la base de datos para el usuario.
    /// </summary>
    private async Task CrearNotificacionAsync(string userId, string titulo, string contenido, string tipo = "info")
    {
        var notif = new Notificacion
        {
            UsuarioId = userId,
            Titulo = titulo,
            Contenido = contenido,
            Tipo = tipo,
            FechaCreacion = DateTime.Now,
            Leida = false
        };
        _context.Notificaciones.Add(notif);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Inicializa el controlador con los servicios de contexto y gestión de identidades.
    /// </summary>
    /// <param name="context">Acceso al modelo de datos.</param>
    /// <param name="userManager">Gestor de usuarios de Identity.</param>
    public MultasController(BibliotecaContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    /// <summary>
    /// Método privado reutilizable: carga una multa con sus relaciones de préstamo y libro.
    /// </summary>
    /// <param name="multaId">ID de la multa a recuperar.</param>
    /// <returns>Entidad Multa con relaciones cargadas o null si no existe.</returns>
    private async Task<Multa> ObtenerMultaDetalladaAsync(int multaId)
    {
        return await _context.Multas
            .Include(m => m.Prestamo)
            .ThenInclude(p => p.Libro)
            .FirstOrDefaultAsync(m => m.Id == multaId);
    }

    /// <summary>
    /// Muestra las deudas y el historial de pagos del usuario autenticado.
    /// </summary>
    /// <returns>Vista con listado de multas personales y sus estados.</returns>
    [Authorize(Roles = "Usuario")]
    public async Task<IActionResult> MisMultas()
    {
        var usuarioId = _userManager.GetUserId(User);

        var multas = await _context.Multas
            .Include(m => m.Prestamo)
            .ThenInclude(p => p.Libro)
            .Where(m => m.Prestamo.UsuarioId == usuarioId)
            .OrderByDescending(m => m.FechaGenerada)
            .ToListAsync();

        return View(multas);
    }

    /// <summary>
    /// Muestra el resumen de la multa y el formulario de pago seguro para el usuario.
    /// Verifica que la multa pertenezca al usuario antes de mostrarse.
    /// </summary>
    /// <param name="id">ID de la multa a pagar.</param>
    [Authorize(Roles = "Usuario")]
    public async Task<IActionResult> Checkout(int id)
    {
        var multa = await ObtenerMultaDetalladaAsync(id);

        if (multa == null || multa.Pagada)
            return NotFound();

        var usuarioId = _userManager.GetUserId(User);
        if (multa.Prestamo?.UsuarioId != usuarioId)
            return Forbid();

        return View(multa);
    }

    /// <summary>
    /// Procesa el pago de una multa mediante la pasarela de pagos segura (Mock).
    /// Valida el número de tarjeta, asocia la transacción al usuario y sanea la deuda.
    /// </summary>
    /// <param name="MultaId">ID de la multa a liquidar.</param>
    /// <param name="NumeroTarjeta">Cadenas de dígitos de la tarjeta (se almacenan solo los últimos 4).</param>
    [HttpPost]
    [Authorize(Roles = "Usuario")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcesarPago(int MultaId, string NumeroTarjeta)
    {
        var multa = await ObtenerMultaDetalladaAsync(MultaId);

        if (multa == null || multa.Pagada)
            return BadRequest("La multa ya fue pagada o no existe.");

        var usuarioId = _userManager.GetUserId(User);
        if (multa.Prestamo?.UsuarioId != usuarioId)
            return Forbid();

        // Simulamos validación de tarjeta (básica)
        string tarjetaLimpia = NumeroTarjeta?.Replace(" ", "") ?? "";
        if (tarjetaLimpia.Length < 13 || tarjetaLimpia.Length > 19)
        {
            TempData["Error"] = "Número de tarjeta inválido. Verifica los dígitos.";
            return RedirectToAction(nameof(Checkout), new { id = MultaId });
        }

        string ultimosDigitos = tarjetaLimpia.Substring(Math.Max(0, tarjetaLimpia.Length - 4));

        var pago = new Pago
        {
            MultaId = multa.Id,
            UsuarioId = usuarioId,
            Monto = multa.Monto,
            FechaPago = DateTime.Now,
            MetodoPago = "Tarjeta de Crédito",
            UltimosDigitosTarjeta = ultimosDigitos
        };

        _context.Pagos.Add(pago);

        multa.Pagada = true;
        multa.FechaPago = DateTime.Now;

        await _context.SaveChangesAsync();

        // 🔔 Notificación Interna
        await CrearNotificacionAsync(usuarioId, "💰 Pago Aprobado", $"Tu pago de ${multa.Monto.ToString("N0")} ha sido procesado. La multa por el libro '{multa.Prestamo.Libro.Titulo}' ha sido saldada.", "success");

        TempData["Success"] = $"¡Pago de ${multa.Monto.ToString("N0")} aprobado exitosamente! Tu multa ha sido saldada.";
        
        return RedirectToAction(nameof(MisMultas));
    }

    /// <summary>
    /// Vista administrativa: Lista todas las multas del sistema con su estado y usuarios asociados.
    /// </summary>
    /// <returns>Vista con el histórico de multas para gestión administrativa.</returns>
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index()
    {
        var multas = await _context.Multas
            .Include(m => m.Prestamo)
            .ThenInclude(p => p.Libro)
            .Include(m => m.Prestamo)
            .ThenInclude(p => p.Usuario)
            .OrderByDescending(m => m.FechaGenerada)
            .ToListAsync();

        return View(multas);
    }

    /// <summary>
    /// Permite al administrador marcar manualmente una multa como pagada.
    /// Útil para conciliaciones en efectivo o correcciones manuales.
    /// </summary>
    /// <param name="id">ID de la multa a marcar como pagada.</param>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarcarPagada(int id)
    {
        var multa = await _context.Multas.FindAsync(id);

        if (multa == null)
            return NotFound();

        multa.Pagada = true;
        multa.FechaPago = DateTime.Now;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Multa pagada correctamente.";

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Genera un reporte administrativo en formato CSV con el histórico de multas.
    /// Incluye datos del usuario, libro, montos y estados de pago.
    /// </summary>
    /// <returns>Archivo descargable con codificación UTF-8 compatible con Excel.</returns>
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ExportarCSV()
    {
        var multas = await _context.Multas
            .Include(m => m.Prestamo)
                .ThenInclude(p => p.Libro)
            .Include(m => m.Prestamo)
                .ThenInclude(p => p.Usuario)
            .OrderByDescending(m => m.FechaGenerada)
            .ToListAsync();

        using (var memoryStream = new MemoryStream())
        using (var writer = new StreamWriter(memoryStream))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteField("ID");
            csv.WriteField("Usuario");
            csv.WriteField("Libro");
            csv.WriteField("Monto");
            csv.WriteField("Fecha Emision");
            csv.WriteField("Estado");
            csv.WriteField("Fecha Pago");
            csv.NextRecord();

            foreach (var m in multas)
            {
                csv.WriteField(m.Id);
                csv.WriteField(m.Prestamo?.Usuario?.NombreCompleto ?? "Anónimo");
                csv.WriteField(m.Prestamo?.Libro?.Titulo ?? "Sin Título");
                csv.WriteField(m.Monto);
                csv.WriteField(m.FechaGenerada.ToString("d"));
                csv.WriteField(m.Pagada ? "Saldada" : "Pendiente");
                csv.WriteField(m.FechaPago?.ToString("d") ?? "-");
                csv.NextRecord();
            }

            writer.Flush();
            var result = memoryStream.ToArray();
            var fileName = $"Reporte_Multas_{DateTime.Now:yyyyMMdd}.csv";
            return File(result, "text/csv", fileName);
        }
    }
}