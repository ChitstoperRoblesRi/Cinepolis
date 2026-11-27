namespace Cinepolis.Domain;

/// <summary>Entidad de folio del sistema de cine.</summary>
public sealed class Folio
{
    public int FolioId { get; init; }

    public required string Codigo { get; init; }

    public required string TipoFolio { get; init; }

    public required string TipoUso { get; init; }

    public required string Estado { get; init; }

    public int? VentaId { get; init; }

    public int? SocioId { get; init; }

    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public FolioTipoFolio TipoFolioTipado => ValoresFolioTipoFolio.DesdeTexto(TipoFolio);

    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public FolioTipoUso TipoUsoTipado => ValoresFolioTipoUso.DesdeTexto(TipoUso);

    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public FolioEstado EstadoTipado => ValoresFolioEstado.DesdeTexto(Estado);
}
