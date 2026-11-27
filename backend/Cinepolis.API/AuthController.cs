using Cinepolis.Application;
using Microsoft.AspNetCore.Mvc;

namespace Cinepolis.API;

/// <summary>Expone las operaciones HTTP de auth.</summary>
[ApiController, Route("api/v1/auth")]
public sealed class AuthController(AccesoClub acceso, TokensClub jwt) : ControllerBase
{
    /// <summary>Solicita el envío de un código de acceso.</summary>
    [HttpPost("codigo")]
    public async Task<IActionResult> Codigo(SolicitarCodigo solicitud, CancellationToken token)
    {
        if (!await acceso.SolicitarCodigoAsync(solicitud, token))
            return Problem(statusCode: 429, title: "Espera un minuto antes de solicitar otro código.");
        return Accepted();
    }

    /// <summary>Registra al socio después de verificar su código.</summary>
    [HttpPost("registro")]
    public async Task<ActionResult<RespuestaLogin>> Registrar(Registro registro, CancellationToken token)
    {
        if (registro.FechaNacimiento > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            ModelState.AddModelError(nameof(registro.FechaNacimiento), "La fecha no puede ser futura.");
            return ValidationProblem(ModelState);
        }
        return await Acceso(new(registro.Correo, registro.Codigo), registro, token);
    }

    /// <summary>Inicia sesión con correo y código de acceso.</summary>
    [HttpPost("login")]
    public Task<ActionResult<RespuestaLogin>> Iniciar(Login login, CancellationToken token) => Acceso(login, null, token);

    private async Task<ActionResult<RespuestaLogin>> Acceso(Login login, Registro? registro, CancellationToken token)
    {
        var socio = await acceso.AutenticarAsync(login, registro, token);
        return socio.HasValue ? Ok(jwt.Emitir(socio.Value)) : Problem(statusCode: 401, title: "Credenciales inválidas.");
    }
}
