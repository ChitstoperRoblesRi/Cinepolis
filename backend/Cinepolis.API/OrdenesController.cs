using System.Security.Cryptography;
using Cinepolis.Application;
using Cinepolis.Domain;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;

namespace Cinepolis.API;

/// <summary>Protege la identidad de la conexión que reserva una butaca.</summary>
public sealed class CredencialReserva(IDataProtectionProvider provider)
{
    private readonly ITimeLimitedDataProtector protector = provider.CreateProtector("Cinepolis.Reserva.v1").ToTimeLimitedDataProtector();
    /// <summary>Emite una credencial temporal para el titular.</summary>
    public string Emitir(string conexion) => protector.Protect(conexion, TimeSpan.FromMinutes(6));
    /// <summary>Recupera la conexión titular de una credencial vigente.</summary>
    public string Abrir(string token) => protector.Unprotect(token);
}

/// <summary>Expone las operaciones HTTP de ordenes.</summary>
[ApiController, Route("api/v1/ordenes")]
public sealed class OrdenesController(ServicioOrdenes ordenes, CredencialReserva reservas) : ControllerBase
{
    private int? Socio => int.TryParse(User.FindFirst("sub")?.Value, out var id) ? id : null;

    /// <summary>Confirma boletos y dulcería en una venta y devuelve su comprobante.</summary>
    [HttpPost(""), HttpPost("checkout")]
    public async Task<ActionResult<TicketDigital>> Comprar(Checkout compra, CancellationToken token)
    {
        if (Request.Headers.ContainsKey("Authorization") && Socio is null) return Unauthorized();
        if (!ValidadorCheckout.LineasValidas(compra))
        {
            ModelState.AddModelError("lineas", "Revisa las líneas de compra. Para solo dulcería indica el complejo.");
            return ValidationProblem(ModelState);
        }
        string conexion;
        try { conexion = compra.Asientos.Length == 0 ? string.Empty : reservas.Abrir(compra.TokenReserva!); }
        catch (CryptographicException) { return Problem(statusCode: 403, title: "La credencial de reserva es inválida o ya expiró."); }
        return Ok(await ordenes.ComprarAsync(compra, conexion, Socio, token));
    }

    /// <summary>Lista los folios de las compras del titular.</summary>
    [HttpGet]
    public async Task<IActionResult> MisBoletos(CancellationToken token) => Socio is int id
        ? Ok(await ordenes.FoliosAsync(id, token)) : Unauthorized();

    /// <summary>Recupera el ticket si el titular puede consultarlo.</summary>
    [HttpGet("{folio}")]
    public async Task<ActionResult<TicketDigital>> Consultar(string folio, CancellationToken token)
    {
        if (!FolioCompra.EsValido(folio)) return NotFound();
        var ticket = await ordenes.LeerAsync(folio, Socio, token);
        return ticket is null ? NotFound() : Ok(ticket);
    }
}
