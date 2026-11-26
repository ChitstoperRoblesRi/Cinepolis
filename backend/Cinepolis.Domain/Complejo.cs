using System.ComponentModel.DataAnnotations.Schema;

namespace Cinepolis.Domain;

[Table("complejo", Schema = "public")]
public sealed class Complejo
{
    [Column("complejo_id", TypeName = "integer")]
    public int ComplejoId { get; init; }

    [Column("vista_id", TypeName = "character varying(50)")]
    public string? VistaId { get; init; }

    [Column("nombre_comercial", TypeName = "character varying(150)")]
    public required string NombreComercial { get; init; }

    [Column("tipo_complejo", TypeName = "character varying(15)")]
    public required string TipoComplejo { get; init; }

    [Column("direccion", TypeName = "character varying(255)")]
    public required string Direccion { get; init; }

    [Column("codigo_postal", TypeName = "character varying(10)")]
    public required string CodigoPostal { get; init; }

    [Column("fecha_apertura", TypeName = "date")]
    public DateOnly FechaApertura { get; init; }

    [Column("slug", TypeName = "character varying(100)")]
    public required string Slug { get; init; }

    [Column("imagen_url", TypeName = "character varying(500)")]
    public string? ImagenUrl { get; init; }

    [Column("ciudad_id", TypeName = "integer")]
    public int CiudadId { get; init; }

    [NotMapped]
    public ComplejoTipoComplejo TipoComplejoTipado => ValoresComplejoTipoComplejo.DesdeTexto(TipoComplejo);
}
