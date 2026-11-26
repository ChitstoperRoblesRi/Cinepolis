using Cinepolis.Application;
using Microsoft.AspNetCore.Mvc;

namespace Cinepolis.API;

[ApiController]
[Route("api/v1/peliculas")]
public sealed class PeliculasController(ConsultarCartelera servicio) : ControllerBase
{
    [HttpGet("{id:int}")]
    [ProducesResponseType<DetallePeliculaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DetallePeliculaDto>> Detalle(int id, CancellationToken token)
    {
        var pelicula = await servicio.DetalleAsync(id, token);
        return pelicula is null ? Problem(statusCode: 404, title: "Película no encontrada.") : Ok(pelicula);
    }
}
