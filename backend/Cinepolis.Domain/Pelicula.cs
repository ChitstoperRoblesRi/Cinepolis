using System.ComponentModel.DataAnnotations.Schema;

namespace Cinepolis.Domain;

[Table("pelicula", Schema = "public")]
public sealed class Pelicula
{
    [Column("pelicula_id", TypeName = "integer")]
    public int PeliculaId { get; init; }

    [Column("vista_id", TypeName = "character varying(50)")]
    public string? VistaId { get; init; }

    [Column("titulo", TypeName = "character varying(200)")]
    public required string Titulo { get; init; }

    [Column("slug", TypeName = "character varying(200)")]
    public required string Slug { get; init; }

    [Column("clasificacion", TypeName = "character varying(5)")]
    public required string Clasificacion { get; init; }

    [Column("genero", TypeName = "character varying(100)")]
    public required string Genero { get; init; }

    [Column("duracion_min", TypeName = "integer")]
    public int DuracionMin { get; init; }

    [Column("sinopsis", TypeName = "text")]
    public required string Sinopsis { get; init; }

    [Column("categoria", TypeName = "character varying(25)")]
    public required string Categoria { get; init; }

    [Column("es_especial", TypeName = "boolean")]
    public bool EsEspecial { get; init; }

    [Column("poster_url", TypeName = "character varying(500)")]
    public string? PosterUrl { get; init; }

    [Column("banner_url", TypeName = "character varying(500)")]
    public string? BannerUrl { get; init; }

    [Column("trailer_url", TypeName = "character varying(500)")]
    public string? TrailerUrl { get; init; }

    [NotMapped]
    public PeliculaClasificacion ClasificacionTipado => ValoresPeliculaClasificacion.DesdeTexto(Clasificacion);

    [NotMapped]
    public PeliculaCategoria CategoriaTipado => ValoresPeliculaCategoria.DesdeTexto(Categoria);
}
