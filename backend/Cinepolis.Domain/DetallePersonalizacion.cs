using System.ComponentModel.DataAnnotations.Schema;

namespace Cinepolis.Domain;

[Table("detalle_personalizacion", Schema = "public")]
public sealed class DetallePersonalizacion
{
    [Column("personalizacion_id", TypeName = "integer")]
    public int PersonalizacionId { get; init; }

    [Column("detalle_carrito_id", TypeName = "integer")]
    public int DetalleCarritoId { get; init; }

    [Column("opcion_id", TypeName = "integer")]
    public int OpcionId { get; init; }

    [Column("cantidad", TypeName = "integer")]
    public int Cantidad { get; init; }

    [Column("porcentaje", TypeName = "numeric(5,2)")]
    public decimal? Porcentaje { get; init; }
}
