using System.Security.Cryptography;

namespace Cinepolis.Application;

/// <summary>Entrega códigos de acceso a sus titulares.</summary>
public interface ICorreoOtp
{
    /// <summary>Envía el código vigente a la dirección indicada.</summary>
    Task EnviarAsync(string correo, string codigo, CancellationToken token);
}

/// <summary>Coordina la emisión y verificación de códigos para el acceso a Club.</summary>
public sealed class AccesoClub(IAccesoClubRepository club, ICorreoOtp correoOtp)
{
    /// <summary>Emite y entrega el código cuando lo permite el intervalo de reenvío.</summary>
    public async Task<bool> SolicitarCodigoAsync(SolicitarCodigo solicitud, CancellationToken token)
    {
        string correo = solicitud.Correo.Trim().ToLowerInvariant();
        string codigo = RandomNumberGenerator.GetInt32(1000000).ToString("D6");
        if (!await club.CrearCodigoAsync(correo, codigo, token))
            return false;
        await correoOtp.EnviarAsync(correo, codigo, token);
        return true;
    }

    /// <summary>Verifica el código y crea la cuenta cuando se solicita el registro.</summary>
    public Task<int?> AutenticarAsync(Login login, Registro? registro, CancellationToken token) =>
        club.AutenticarAsync(login, registro, token);
}
