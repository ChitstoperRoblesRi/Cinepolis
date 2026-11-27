namespace Cinepolis.Domain;

/// <summary>Entidad de banner promo del sistema de cine.</summary>
public sealed class BannerPromo
{
    public int BannerId { get; init; }

    public required string Titulo { get; init; }

    public required string ImagenUrl { get; init; }

    public required string UrlDestino { get; init; }

    public DateOnly FechaInicio { get; init; }

    public DateOnly? FechaFin { get; init; }

    public bool Activo { get; init; }

    public bool AplicaTodos { get; init; }
}
