using System.ComponentModel.DataAnnotations.Schema;

namespace Cinepolis.Domain;

[Table("modificador_regla", Schema = "public")]
public sealed class ModificadorRegla
{
    [Column("regla_id", TypeName = "integer")]
    public int ReglaId { get; init; }

    [Column("producto_id", TypeName = "integer")]
    public int ProductoId { get; init; }

    [Column("titulo_regla", TypeName = "character varying(150)")]
    public required string TituloRegla { get; init; }

    [Column("tipo_modificador", TypeName = "character varying(50)")]
    public required string TipoModificador { get; init; }

    [Column("min_items", TypeName = "integer")]
    public int MinItems { get; init; }

    [Column("max_items", TypeName = "integer")]
    public int MaxItems { get; init; }

    [Column("grupo_titulo", TypeName = "text")]
    public string? GrupoTitulo { get; init; }

    [Column("grupo_orden", TypeName = "integer")]
    public int? GrupoOrden { get; init; }

    [NotMapped]
    public ModificadorReglaTipoModificador TipoModificadorTipado => ValoresModificadorReglaTipoModificador.DesdeTexto(TipoModificador);
}
