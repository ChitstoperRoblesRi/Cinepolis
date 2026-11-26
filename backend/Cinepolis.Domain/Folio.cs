using System.ComponentModel.DataAnnotations.Schema;

namespace Cinepolis.Domain;

[Table("folio", Schema = "public")]
public sealed class Folio
{
    [Column("folio_id", TypeName = "integer")]
    public int FolioId { get; init; }

    [Column("codigo", TypeName = "character varying(100)")]
    public required string Codigo { get; init; }

    [Column("tipo_folio", TypeName = "character varying(20)")]
    public required string TipoFolio { get; init; }

    [Column("tipo_uso", TypeName = "character varying(10)")]
    public required string TipoUso { get; init; }

    [Column("estado", TypeName = "character varying(10)")]
    public required string Estado { get; init; }

    [Column("venta_id", TypeName = "integer")]
    public int? VentaId { get; init; }

    [Column("socio_id", TypeName = "integer")]
    public int? SocioId { get; init; }

    [NotMapped]
    public FolioTipoFolio TipoFolioTipado => ValoresFolioTipoFolio.DesdeTexto(TipoFolio);

    [NotMapped]
    public FolioTipoUso TipoUsoTipado => ValoresFolioTipoUso.DesdeTexto(TipoUso);

    [NotMapped]
    public FolioEstado EstadoTipado => ValoresFolioEstado.DesdeTexto(Estado);
}
