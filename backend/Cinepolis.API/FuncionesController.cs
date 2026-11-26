using Cinepolis.Application;
using Microsoft.AspNetCore.Mvc;

namespace Cinepolis.API;

[ApiController]
[Route("api/v1/funciones")]
public sealed class FuncionesController(ConsultarMapaSala consulta) : ControllerBase
{
    [HttpGet("{funcionId:int}/asientos")]
    public async Task<ActionResult<MapaSala>> Asientos(int funcionId, CancellationToken token)
    {
        var mapa = await consulta.EjecutarAsync(funcionId, token);
        return mapa is null ? Problem(statusCode: 404, title: "Función no encontrada.") : Ok(mapa);
    }
}
