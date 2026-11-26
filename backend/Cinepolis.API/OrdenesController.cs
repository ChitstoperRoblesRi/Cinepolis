using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Cinepolis.Application;
using Cinepolis.Domain;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;

namespace Cinepolis.API;

public sealed class CredencialReserva(IDataProtectionProvider provider)
{
    private readonly ITimeLimitedDataProtector protector = provider.CreateProtector("Cinepolis.Reserva.v1").ToTimeLimitedDataProtector();
    public string Emitir(string conexion) => protector.Protect(conexion, TimeSpan.FromMinutes(6));
    public string Abrir(string token) => protector.Unprotect(token);
}

public sealed record TicketDigital(Ticket Ticket, string QrPayload)
{
    public string Folio => Ticket.Orden.Folio;
    public string ValidacionQr => QrPayload;
}

[ApiController, Route("api/v1/ordenes")]
public sealed class OrdenesController(IOrdenRepository ordenes, CredencialReserva reservas, IDataProtectionProvider provider,
    INotificadorSala sala, ILogger<OrdenesController> log) : ControllerBase
{
    private readonly IDataProtector qr = provider.CreateProtector("Cinepolis.TicketQR.v1");
    private int? Socio => int.TryParse(User.FindFirst("sub")?.Value, out var id) ? id : null;

    [HttpPost("checkout")]
    public async Task<ActionResult<TicketDigital>> Comprar(Checkout compra, CancellationToken token)
    {
        if (Request.Headers.ContainsKey("Authorization") && Socio is null) return Unauthorized();
        if (compra.Asientos.Any(b => b is null) || compra.Productos.Any(p => p is null)
            || compra.Asientos.DistinctBy(b => b.AsientoId).Count() != compra.Asientos.Length)
        {
            ModelState.AddModelError("lineas", "Cada asiento debe aparecer una sola vez y las líneas deben ser válidas.");
            return ValidationProblem(ModelState);
        }
        string conexion;
        try { conexion = reservas.Abrir(compra.TokenReserva); }
        catch (CryptographicException) { return Problem(statusCode: 403, title: "La credencial de reserva es inválida o ya expiró."); }
        var ticket = await ordenes.ComprarAsync(compra, conexion, Socio, token);
        foreach (var boleto in ticket.Boletos)
        {
            try { await sala.PublicarAsync(new(boleto.FuncionId, boleto.AsientoId, AsientoEstado.Vendido.TextoSql()), CancellationToken.None); }
            catch (Exception ex) { log.LogError(ex, "Fallo al notificar la venta del asiento {AsientoId}", boleto.AsientoId); }
        }
        return Ok(Digital(ticket));
    }

    [HttpGet]
    public async Task<IActionResult> MisBoletos(CancellationToken token) => Socio is int id
        ? Ok(await ordenes.FoliosAsync(id, token)) : Unauthorized();

    [HttpGet("{folio}")]
    public async Task<ActionResult<TicketDigital>> Consultar(string folio, CancellationToken token)
    {
        if (!Regex.IsMatch(folio, @"\AWEB-[0-9A-F]{40}\z")) return NotFound();
        var ticket = await ordenes.LeerAsync(folio, Socio, token);
        return ticket is null ? NotFound() : Ok(Digital(ticket));
    }

    private TicketDigital Digital(Ticket ticket) => new(ticket, qr.Protect(ticket.Orden.Folio));
}
