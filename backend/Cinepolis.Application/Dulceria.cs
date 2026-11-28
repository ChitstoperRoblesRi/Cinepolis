using Cinepolis.Domain;

namespace Cinepolis.Application;

/// <summary>Identidad y nombre de una categoría de dulcería.</summary>
public sealed record CategoriaDto(int CategoriaId, string NombreCategoria);
/// <summary>Opción seleccionable y ajuste de precio del catálogo.</summary>
public sealed record OpcionDto(int OpcionId, string Nombre, decimal Extra, decimal? PrecioVariante);
/// <summary>Límites de selección y opciones de un componente.</summary>
public sealed record ReglaDto(int ReglaId, string Titulo, int Min, int Max, OpcionDto[] Opciones,
    string? GrupoTitulo, string TipoModificador, int? BloquePalomitas = null,
    IReadOnlyDictionary<int, int>? MaximosPorTamano = null)
{
    public bool SaboresPalomitas => BloquePalomitas.HasValue && TipoModificador == "flavor";
}
/// <summary>Presentación de dulcería y reglas aplicables en su catálogo.</summary>
public sealed record ProductoDto(int ProductoId, int CategoriaId, string NombreProducto, string Estructura,
    string? Descripcion, string? ImagenUrl, int? CatalogoId, decimal? Precio, ReglaDto[] Reglas);
/// <summary>Productos y combos agrupados por categoría.</summary>
public sealed record CategoriaDulceriaDto(int CategoriaId, string NombreCategoria,
    IReadOnlyList<ProductoDto> Productos, IReadOnlyList<ProductoDto> Combos);

/// <summary>Recupera el catálogo regional de productos y combos.</summary>
public interface IDulceriaCatalogoRepository
{
    /// <summary>Recupera categorías y productos para el complejo indicado.</summary>
    Task<(IReadOnlyList<CategoriaDto> Categorias, IReadOnlyList<ProductoDto> Productos)> CatalogoAsync(int? complejoId, CancellationToken token);
}

/// <summary>Recupera el catálogo regional de productos y combos.</summary>
public sealed class ConsultarDulceria(IDulceriaCatalogoRepository repository)
{
    /// <summary>Devuelve los resultados disponibles para los filtros indicados.</summary>
    public async Task<IReadOnlyList<CategoriaDulceriaDto>> ListarAsync(int? complejoId, CancellationToken token)
    {
        var (categorias, productos) = await repository.CatalogoAsync(complejoId, token);
        var grupos = productos.ToLookup(p => (p.CategoriaId, EsCombo: p.Estructura == ProductoDulceriaEstructura.Combo.TextoSql()));
        return categorias.Select(c => new CategoriaDulceriaDto(c.CategoriaId, c.NombreCategoria,
            grupos[(c.CategoriaId, false)].ToArray(), grupos[(c.CategoriaId, true)].ToArray())).ToArray();
    }
}
