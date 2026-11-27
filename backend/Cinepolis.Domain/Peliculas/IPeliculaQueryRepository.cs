namespace Cinepolis.Domain;

/// <summary>Ficha técnica con sinopsis, directores y reparto.</summary>
public sealed record FichaPelicula(int PeliculaId, string Titulo, string Clasificacion, string Sinopsis,
    string? TrailerUrl, string? PosterUrl, string Genero, int DuracionMin, string[] Directores, string[] Reparto)
{
    /// <summary>Ficha técnica con sinopsis, directores y reparto.</summary>
    public FichaPelicula() : this(0, string.Empty, string.Empty, string.Empty, null, null, string.Empty, 0, [], []) { }
}
/// <summary>Consulta la ficha técnica y los créditos de la película.</summary>
public interface IPeliculaQueryRepository
{
    /// <summary>Recupera la ficha de la película solicitada.</summary>
    Task<FichaPelicula?> FichaAsync(int id, CancellationToken token);
}
