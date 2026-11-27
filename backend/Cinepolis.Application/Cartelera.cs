using Cinepolis.Domain;

namespace Cinepolis.Application;

/// <summary>Criterios opcionales de búsqueda de funciones.</summary>
public sealed record FiltrosCartelera(int? ComplejoId = null, DateOnly? Fecha = null, string? Formato = null,
    string? Idioma = null, string? Clasificacion = null);
/// <summary>Proyección de una función programada en cartelera.</summary>
public sealed record FuncionDto(int FuncionId, int ComplejoId, string NombreComercial, int SalaId,
    string NombreSala, string Idioma, TimeOnly HoraTermino, bool EsPreventa);
/// <summary>Funciones disponibles en una fecha y hora de inicio.</summary>
public sealed record HorarioDto(DateOnly Fecha, TimeOnly HoraInicio, IReadOnlyList<FuncionDto> Funciones);
/// <summary>Horarios agrupados por formato de proyección y tipo de sala.</summary>
public sealed record FormatoDto(string TipoSala, string NombreFormato, IReadOnlyList<HorarioDto> Horarios);
/// <summary>Película de cartelera con los formatos y horarios disponibles.</summary>
public sealed record CarteleraDto(int PeliculaId, string Titulo, string Clasificacion, string? PosterUrl,
    IReadOnlyList<FormatoDto> Formatos, string Genero, int DuracionMin, string Categoria, string? BannerUrl, string? TrailerUrl);
/// <summary>Ficha pública de la película y su programación.</summary>
public sealed record DetallePeliculaDto(int PeliculaId, string Titulo, string Clasificacion, string Sinopsis,
    string? TrailerUrl, string? PosterUrl, string Genero, int DuracionMin, IReadOnlyList<FormatoDto> Formatos,
    IReadOnlyList<string> Directores, IReadOnlyList<string> Reparto);
/// <summary>Proyección de una función programada en cartelera.</summary>
public sealed record FuncionCartelera(int PeliculaId, string Titulo, string Clasificacion, string? PosterUrl,
    string TipoSala, string NombreFormato, DateOnly FechaFuncion, TimeOnly HoraInicio, int FuncionId,
    int ComplejoId, string NombreComercial, int SalaId, string NombreSala, string Idioma, TimeOnly HoraTermino,
    bool EsPreventa, string Genero, int DuracionMin, string Categoria, string? BannerUrl, string? TrailerUrl);

/// <summary>Consulta fichas técnicas y funciones de cartelera.</summary>
public interface IFuncionesCarteleraRepository
{
    /// <summary>Obtiene las funciones de las películas según los filtros.</summary>
    Task<IReadOnlyList<FuncionCartelera>> FuncionesAsync(FiltrosCartelera filtros, int? peliculaId, CancellationToken token);

}

/// <summary>Consulta fichas técnicas y funciones de cartelera.</summary>
public sealed class ConsultarCartelera(IPeliculaQueryRepository peliculas, IFuncionesCarteleraRepository funcionesCartelera)
{
    /// <summary>Devuelve los resultados disponibles para los filtros indicados.</summary>
    public async Task<IReadOnlyList<CarteleraDto>> ListarAsync(FiltrosCartelera filtros, CancellationToken token)
    {
        var filas = await funcionesCartelera.FuncionesAsync(filtros, null, token);
        return filas.GroupBy(f => new { f.PeliculaId, f.Titulo, f.Clasificacion, f.PosterUrl, f.Genero, f.DuracionMin, f.Categoria, f.BannerUrl, f.TrailerUrl })
            .Select(g => new CarteleraDto(g.Key.PeliculaId, g.Key.Titulo, g.Key.Clasificacion, g.Key.PosterUrl, Agrupar(g),
                g.Key.Genero, g.Key.DuracionMin, g.Key.Categoria, g.Key.BannerUrl, g.Key.TrailerUrl))
            .ToArray();
    }

    /// <summary>Recupera la ficha de la película solicitada.</summary>
    public async Task<DetallePeliculaDto?> DetalleAsync(int id, CancellationToken token)
    {
        var ficha = await peliculas.FichaAsync(id, token);
        if (ficha is null) return null;
        var funciones = await funcionesCartelera.FuncionesAsync(new(), id, token);
        return new(ficha.PeliculaId, ficha.Titulo, ficha.Clasificacion, ficha.Sinopsis, ficha.TrailerUrl,
            ficha.PosterUrl, ficha.Genero, ficha.DuracionMin, Agrupar(funciones), ficha.Directores, ficha.Reparto);
    }

    private static FormatoDto[] Agrupar(IEnumerable<FuncionCartelera> filas) =>
        filas.GroupBy(f => new { f.TipoSala, f.NombreFormato })
            .Select(formato => new FormatoDto(formato.Key.TipoSala, formato.Key.NombreFormato,
                formato.GroupBy(f => new { f.FechaFuncion, f.HoraInicio })
                    .Select(h => new HorarioDto(h.Key.FechaFuncion, h.Key.HoraInicio,
                        h.Select(f => new FuncionDto(f.FuncionId, f.ComplejoId, f.NombreComercial, f.SalaId,
                            f.NombreSala, f.Idioma, f.HoraTermino, f.EsPreventa)).ToArray())).ToArray())).ToArray();
}
