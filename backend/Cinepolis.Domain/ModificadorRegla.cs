namespace Cinepolis.Domain;

/// <summary>Entidad de modificador regla del sistema de cine.</summary>
public sealed class ModificadorRegla
{
    public int ReglaId { get; init; }

    public int ProductoId { get; init; }

    public required string TituloRegla { get; init; }

    public required string TipoModificador { get; init; }

    public int MinItems { get; init; }

    public int MaxItems { get; init; }

    public string? GrupoTitulo { get; init; }

    public int? GrupoOrden { get; init; }

    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public ModificadorReglaTipoModificador TipoModificadorTipado => ValoresModificadorReglaTipoModificador.DesdeTexto(TipoModificador);
}
