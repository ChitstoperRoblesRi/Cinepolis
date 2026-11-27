namespace Cinepolis.Domain;

/// <summary>Entidad de pelicula del sistema de cine.</summary>
public sealed class Pelicula
{
    public int PeliculaId { get; init; }

    public string? VistaId { get; init; }

    public required string Titulo { get; init; }

    public required string Slug { get; init; }

    public required string Clasificacion { get; init; }

    public required string Genero { get; init; }

    public int DuracionMin { get; init; }

    public required string Sinopsis { get; init; }

    public required string Categoria { get; init; }

    public bool EsEspecial { get; init; }

    public string? PosterUrl { get; init; }

    public string? BannerUrl { get; init; }

    public string? TrailerUrl { get; init; }

    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public PeliculaClasificacion ClasificacionTipado => ValoresPeliculaClasificacion.DesdeTexto(Clasificacion);

    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public PeliculaCategoria CategoriaTipado => ValoresPeliculaCategoria.DesdeTexto(Categoria);
}
