using Cinepolis.Application;
using Microsoft.AspNetCore.Mvc;

namespace Cinepolis.API;

[ApiController]
[Route("api/v1/dulceria")]
public sealed class DulceriaController(ConsultarDulceria servicio) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<CategoriaDulceriaDto>> Listar(int? complejoId, CancellationToken token) => servicio.ListarAsync(complejoId, token);
}
