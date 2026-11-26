using Cinepolis.Application;
using Microsoft.AspNetCore.Mvc;

namespace Cinepolis.API;

[ApiController]
[Route("api/v1")]
public sealed class ComplejosController(ConsultarUbicaciones servicio) : ControllerBase
{
    [HttpGet("ciudades")]
    public Task<IReadOnlyList<CiudadDto>> Ciudades(CancellationToken token) => servicio.CiudadesAsync(token);

    [HttpGet("complejos")]
    public Task<IReadOnlyList<ComplejoDto>> Complejos([FromQuery] int? ciudadId, CancellationToken token) =>
        servicio.ComplejosAsync(ciudadId, token);
}
