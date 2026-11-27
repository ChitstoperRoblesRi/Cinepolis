namespace Cinepolis.Domain;

/// <summary>Entidad de nivel club del sistema de cine.</summary>
public sealed class NivelClub
{
    public int NivelId { get; init; }

    public required string NombreNivel { get; init; }

    public int VisitasMinSemestre { get; init; }

    public int? VisitasMaxSemestre { get; init; }

    public decimal PorcentajeAcumulacionPuntos { get; init; }

    public string? DescripcionBeneficios { get; init; }
}
