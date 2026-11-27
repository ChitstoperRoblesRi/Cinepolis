using Cinepolis.Application;
using Microsoft.AspNetCore.Mvc;

namespace Cinepolis.API;

/// <summary>Expone las operaciones HTTP de dulceria.</summary>
[ApiController]
[Route("api/v1/dulceria")]
public sealed class DulceriaController(ConsultarDulceria servicio) : ControllerBase
{
    /// <summary>Devuelve los resultados disponibles para los filtros indicados.</summary>
    [HttpGet]
    public Task<IReadOnlyList<CategoriaDulceriaDto>> Listar(int? complejoId, CancellationToken token) => servicio.ListarAsync(complejoId, token);
}
