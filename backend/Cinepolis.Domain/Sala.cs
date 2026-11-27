namespace Cinepolis.Domain;

/// <summary>Entidad de sala del sistema de cine.</summary>
public sealed class Sala
{
    public int SalaId { get; init; }

    public int NumeroSala { get; init; }

    public required string NombreSala { get; init; }

    public int CapacidadAsientos { get; init; }

    public required string TipoSala { get; init; }

    public int ComplejoId { get; init; }

    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public PrecioBoletoTipoSala TipoSalaTipado => ValoresPrecioBoletoTipoSala.DesdeTexto(TipoSala);
}
