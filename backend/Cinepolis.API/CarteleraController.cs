using Cinepolis.Application;
using Microsoft.AspNetCore.Mvc;

namespace Cinepolis.API;

[ApiController]
[Route("api/v1/cartelera")]
public sealed class CarteleraController(ConsultarCartelera servicio) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<CarteleraDto>> Listar([FromQuery] FiltrosCartelera filtros, CancellationToken token) =>
        servicio.ListarAsync(filtros, token);
}
