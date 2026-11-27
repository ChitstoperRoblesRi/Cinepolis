namespace Cinepolis.Domain;

/// <summary>Entidad de otp verificacion del sistema de cine.</summary>
public sealed class OtpVerificacion
{
    public int OtpId { get; init; }

    public required string Correo { get; init; }

    public required string Codigo { get; init; }

    public int Intentos { get; init; }

    public DateTime Expiracion { get; init; }

    public bool Usado { get; init; }
}
