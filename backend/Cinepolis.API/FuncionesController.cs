using Cinepolis.Application;
using Microsoft.AspNetCore.Mvc;

namespace Cinepolis.API;

/// <summary>Expone las operaciones HTTP de funciones.</summary>
[ApiController]
[Route("api/v1/funciones")]
public sealed class FuncionesController(ConsultarMapaSala consulta) : ControllerBase
{
    /// <summary>Recupera la geometría, disponibilidad y tarifas de la sala.</summary>
    [HttpGet("{funcionId:int}/asientos")]
    public async Task<ActionResult<MapaSala>> Asientos(int funcionId, CancellationToken token)
    {
        var mapa = await consulta.EjecutarAsync(funcionId, token);
        return mapa is null ? Problem(statusCode: 404, title: "Función no encontrada.") : Ok(mapa);
    }
}
