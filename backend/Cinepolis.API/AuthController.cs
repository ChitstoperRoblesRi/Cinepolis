using System.Security.Cryptography;
using Cinepolis.Application;
using Microsoft.AspNetCore.Mvc;

namespace Cinepolis.API;

[ApiController, Route("api/v1/auth")]
public sealed class AuthController(IClubRepository club, TokensClub jwt, CorreoOtp correoOtp) : ControllerBase
{
    [HttpPost("codigo")]
    public async Task<IActionResult> Codigo(SolicitarCodigo solicitud, CancellationToken token)
    {
        string correo = solicitud.Correo.Trim().ToLowerInvariant();
        string codigo = RandomNumberGenerator.GetInt32(1000000).ToString("D6");
        if (!await club.CrearCodigoAsync(correo, codigo, token))
            return Problem(statusCode: 429, title: "Espera un minuto antes de solicitar otro código.");
        await correoOtp.EnviarAsync(correo, codigo, token);
        return Accepted();
    }

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

    [HttpPost("login")]
    public Task<ActionResult<RespuestaLogin>> Iniciar(Login login, CancellationToken token) => Acceso(login, null, token);

    private async Task<ActionResult<RespuestaLogin>> Acceso(Login login, Registro? registro, CancellationToken token)
    {
        var socio = await club.AutenticarAsync(login, registro, token);
        return socio.HasValue ? Ok(jwt.Emitir(socio.Value)) : Problem(statusCode: 401, title: "Credenciales inválidas.");
    }
}
