using Cinepolis.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cinepolis.API;

/// <summary>Expone las operaciones HTTP de club.</summary>
[Authorize, ApiController, Route("api/v1/club")]
public sealed class ClubController(ConsultarClub club) : ControllerBase
{
    /// <summary>Recupera el historial de compras del socio autenticado.</summary>
    [HttpGet("compras")]
    public async Task<ActionResult<IEnumerable<CompraClub>>> Compras(CancellationToken token)
    {
        if (!int.TryParse(User.FindFirst("sub")?.Value, out var socioId)) return Unauthorized();
        return Ok(await club.ComprasAsync(socioId, token));
    }

    /// <summary>Recupera los datos y beneficios de la cuenta del socio.</summary>
    [HttpGet("perfil")]
    public async Task<ActionResult<PerfilClub>> Perfil(CancellationToken token)
    {
        if (!int.TryParse(User.FindFirst("sub")?.Value, out var socioId)) return Unauthorized();
        var perfil = await club.PerfilAsync(socioId, token);
        return perfil is null ? NotFound() : Ok(perfil);
    }
}
