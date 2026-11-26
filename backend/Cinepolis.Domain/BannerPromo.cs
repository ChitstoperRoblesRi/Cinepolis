using System.ComponentModel.DataAnnotations.Schema;

namespace Cinepolis.Domain;

[Table("banner_promo", Schema = "public")]
public sealed class BannerPromo
{
    [Column("banner_id", TypeName = "integer")]
    public int BannerId { get; init; }

    [Column("titulo", TypeName = "character varying(150)")]
    public required string Titulo { get; init; }

    [Column("imagen_url", TypeName = "character varying(500)")]
    public required string ImagenUrl { get; init; }

    [Column("url_destino", TypeName = "character varying(500)")]
    public required string UrlDestino { get; init; }

    [Column("fecha_inicio", TypeName = "date")]
    public DateOnly FechaInicio { get; init; }

    [Column("fecha_fin", TypeName = "date")]
    public DateOnly? FechaFin { get; init; }

    [Column("activo", TypeName = "boolean")]
    public bool Activo { get; init; }

    [Column("aplica_todos", TypeName = "boolean")]
    public bool AplicaTodos { get; init; }
}
