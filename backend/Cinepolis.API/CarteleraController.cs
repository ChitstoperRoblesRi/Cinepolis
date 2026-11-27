using Cinepolis.Application;
using Microsoft.AspNetCore.Mvc;

namespace Cinepolis.API;

/// <summary>Expone las operaciones HTTP de cartelera.</summary>
[ApiController]
[Route("api/v1/cartelera")]
public sealed class CarteleraController(ConsultarCartelera servicio) : ControllerBase
{
    /// <summary>Devuelve los resultados disponibles para los filtros indicados.</summary>
    [HttpGet]
    public Task<IReadOnlyList<CarteleraDto>> Listar([FromQuery] FiltrosCartelera filtros, CancellationToken token) =>
        servicio.ListarAsync(filtros, token);
}
