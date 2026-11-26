namespace Cinepolis.Application;

public sealed record FiltrosCartelera(int? ComplejoId = null, DateOnly? Fecha = null, string? Formato = null,
    string? Idioma = null, string? Clasificacion = null);
public sealed record FuncionDto(int FuncionId, int ComplejoId, string NombreComercial, int SalaId,
    string NombreSala, string Idioma, TimeOnly HoraTermino, bool EsPreventa);
public sealed record HorarioDto(DateOnly Fecha, TimeOnly HoraInicio, IReadOnlyList<FuncionDto> Funciones);
public sealed record FormatoDto(string TipoSala, string NombreFormato, IReadOnlyList<HorarioDto> Horarios);
public sealed record CarteleraDto(int PeliculaId, string Titulo, string Clasificacion, string? PosterUrl,
    IReadOnlyList<FormatoDto> Formatos, string Genero, int DuracionMin, string Categoria, string? BannerUrl, string? TrailerUrl);
public sealed record FichaPelicula(int PeliculaId, string Titulo, string Clasificacion, string Sinopsis,
    string? TrailerUrl, string? PosterUrl, string Genero, int DuracionMin, string[] Directores, string[] Reparto)
{
    public FichaPelicula() : this(0, string.Empty, string.Empty, string.Empty, null, null, string.Empty, 0, [], []) { }
}
public sealed record DetallePeliculaDto(int PeliculaId, string Titulo, string Clasificacion, string Sinopsis,
    string? TrailerUrl, string? PosterUrl, string Genero, int DuracionMin, IReadOnlyList<FormatoDto> Formatos,
    IReadOnlyList<string> Directores, IReadOnlyList<string> Reparto);
public sealed record FuncionCartelera(int PeliculaId, string Titulo, string Clasificacion, string? PosterUrl,
    string TipoSala, string NombreFormato, DateOnly FechaFuncion, TimeOnly HoraInicio, int FuncionId,
    int ComplejoId, string NombreComercial, int SalaId, string NombreSala, string Idioma, TimeOnly HoraTermino,
    bool EsPreventa, string Genero, int DuracionMin, string Categoria, string? BannerUrl, string? TrailerUrl);

public interface ICarteleraRepository
{
    Task<IReadOnlyList<FuncionCartelera>> FuncionesAsync(FiltrosCartelera filtros, int? peliculaId, CancellationToken token);
    Task<FichaPelicula?> FichaAsync(int id, CancellationToken token);
}

public sealed class ConsultarCartelera(ICarteleraRepository repository)
{
    public async Task<IReadOnlyList<CarteleraDto>> ListarAsync(FiltrosCartelera filtros, CancellationToken token)
    {
        var filas = await repository.FuncionesAsync(filtros, null, token);
        return filas.GroupBy(f => new { f.PeliculaId, f.Titulo, f.Clasificacion, f.PosterUrl, f.Genero, f.DuracionMin, f.Categoria, f.BannerUrl, f.TrailerUrl })
            .Select(g => new CarteleraDto(g.Key.PeliculaId, g.Key.Titulo, g.Key.Clasificacion, g.Key.PosterUrl, Agrupar(g),
                g.Key.Genero, g.Key.DuracionMin, g.Key.Categoria, g.Key.BannerUrl, g.Key.TrailerUrl))
            .ToArray();
    }

    public async Task<DetallePeliculaDto?> DetalleAsync(int id, CancellationToken token)
    {
        var ficha = await repository.FichaAsync(id, token);
        if (ficha is null) return null;
        var funciones = await repository.FuncionesAsync(new(), id, token);
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
