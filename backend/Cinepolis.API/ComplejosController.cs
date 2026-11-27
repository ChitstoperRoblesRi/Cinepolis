using Cinepolis.Application;
using Microsoft.AspNetCore.Mvc;

namespace Cinepolis.API;

/// <summary>Expone las operaciones HTTP de complejos.</summary>
[ApiController]
[Route("api/v1")]
public sealed class ComplejosController(ConsultarUbicaciones servicio) : ControllerBase
{
    /// <summary>Lista las ciudades con presencia comercial.</summary>
    [HttpGet("ciudades")]
    public Task<IReadOnlyList<CiudadDto>> Ciudades(CancellationToken token) => servicio.CiudadesAsync(token);

    /// <summary>Lista los complejos según la ciudad solicitada.</summary>
    [HttpGet("complejos")]
    public Task<IReadOnlyList<ComplejoDto>> Complejos([FromQuery] int? ciudadId, CancellationToken token) =>
        servicio.ComplejosAsync(ciudadId, token);
}
