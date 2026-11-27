namespace Cinepolis.Domain;

/// <summary>Entidad de cuenta club del sistema de cine.</summary>
public sealed class CuentaClub
{
    public int SocioId { get; init; }

    public required string UsuarioCc { get; init; }

    public required string NumeroTarjeta { get; init; }

    public required string Nombre { get; init; }

    public required string Apellidos { get; init; }

    public required string Correo { get; init; }

    public string? Telefono { get; init; }

    public DateOnly FechaNacimiento { get; init; }

    public string? Genero { get; init; }

    public string? CodigoPostal { get; init; }

    public DateOnly FechaRegistro { get; init; }

    public int PuntosAcumulados { get; init; }

    public DateOnly PuntosVencenFecha { get; init; }

    public int VisitasSemestreActual { get; init; }

    public required string SemestreActual { get; init; }

    public int NivelId { get; init; }

    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public CuentaClubGenero? GeneroTipado => Genero is null ? null : ValoresCuentaClubGenero.DesdeTexto(Genero);
}
