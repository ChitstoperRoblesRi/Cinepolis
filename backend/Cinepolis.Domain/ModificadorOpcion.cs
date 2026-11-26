using System.ComponentModel.DataAnnotations.Schema;

namespace Cinepolis.Domain;

[Table("modificador_opcion", Schema = "public")]
public sealed class ModificadorOpcion
{
    [Column("opcion_id", TypeName = "integer")]
    public int OpcionId { get; init; }

    [Column("regla_id", TypeName = "integer")]
    public int ReglaId { get; init; }

    [Column("api_id_opcion", TypeName = "character varying(50)")]
    public required string ApiIdOpcion { get; init; }

    [Column("nombre_opcion", TypeName = "character varying(100)")]
    public required string NombreOpcion { get; init; }

    [Column("precio_adicional", TypeName = "numeric(8,2)")]
    public decimal PrecioAdicional { get; init; }

    [Column("imagen_url", TypeName = "text")]
    public string? ImagenUrl { get; init; }

    [Column("es_default", TypeName = "boolean")]
    public bool? EsDefault { get; init; }
}
