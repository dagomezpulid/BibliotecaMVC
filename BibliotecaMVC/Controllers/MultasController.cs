using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class MultasController : Controller
{
    private readonly BibliotecaContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public MultasController(BibliotecaContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    private async Task<Multa> ObtenerMultaDetalladaAsync(int multaId)
    {
        return await _context.Multas
            .Include(m => m.Prestamo)
            .ThenInclude(p => p.Libro)
            .FirstOrDefaultAsync(m => m.Id == multaId);
    }

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

        TempData["Success"] = $"¡Pago de ${multa.Monto.ToString("N0")} aprobado exitosamente! Tu multa ha sido saldada.";
        
        return RedirectToAction(nameof(MisMultas));
    }

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



}