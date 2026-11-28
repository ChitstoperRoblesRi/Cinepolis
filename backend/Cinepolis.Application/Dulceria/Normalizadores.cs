namespace Cinepolis.Application;

/// <summary>Identificadores externos que distinguen presentaciones importadas.</summary>
public sealed record DatosCatalogo(string? SkuProducto, bool EsCombo, IReadOnlyDictionary<int, string> SkuOpciones);

/// <summary>Transforma un catálogo conservando sus identificadores de compra.</summary>
public interface INormalizadorCatalogoHandler
{
    /// <summary>Transforma el producto recibido de la etapa anterior.</summary>
    ProductoDto Normalizar(ProductoDto producto, DatosCatalogo datos);
}

/// <summary>Aplica en orden las etapas registradas para consulta y checkout.</summary>
public sealed class NormalizadorCatalogo(IEnumerable<INormalizadorCatalogoHandler> etapas)
{
    /// <summary>Devuelve las reglas normalizadas del producto.</summary>
    public ProductoDto Normalizar(ProductoDto producto, DatosCatalogo datos) =>
        etapas.Aggregate(producto, (actual, etapa) => etapa.Normalizar(actual, datos));
}

/// <summary>Recupera la capacidad de agua embotellada a partir de la presentación de origen.</summary>
public sealed class NormalizadorCapacidadesHandler : INormalizadorCatalogoHandler
{
    /// <inheritdoc/>
    public ProductoDto Normalizar(ProductoDto producto, DatosCatalogo datos) => producto with
    {
        Reglas = producto.Reglas.Select(r => r.TipoModificador != "size" ? r : r with
        {
            Opciones = r.Opciones.Select(o => datos.SkuOpciones[o.OpcionId] == "275" &&
                o.Nombre.Trim().Equals("agua embotellada", StringComparison.OrdinalIgnoreCase)
                ? o with { Nombre = "1 lt" } : o).ToArray()
        }).ToArray()
    };
}

/// <summary>Corrige etiquetas importadas y ordena palomitas, bebidas y complementos del combo.</summary>
public sealed class NormalizadorCombosHandler : INormalizadorCatalogoHandler
{
    /// <inheritdoc/>
    public ProductoDto Normalizar(ProductoDto producto, DatosCatalogo datos)
    {
        var reglas = producto.Reglas.Select(r =>
        {
            var opciones = r.Opciones.Select(o => datos.SkuProducto == "16454" &&
                o.Nombre.Trim().Equals("helado micha no usar", StringComparison.OrdinalIgnoreCase)
                ? o with { Nombre = "M&M's®" } : o);
            return r with { Opciones = opciones.ToArray() };
        });
        if (datos.EsCombo)
            reglas = NumerarComponentes(reglas.OrderBy(OrdenComponente).ToArray());
        if (producto.NombreProducto.Contains("maxicombo", StringComparison.OrdinalIgnoreCase))
            reglas = reglas.GroupBy(r => r.GrupoTitulo).SelectMany(DesglosarHotDog);
        return producto with { Reglas = reglas.ToArray() };
    }

    private static IEnumerable<ReglaDto> DesglosarHotDog(IGrouping<string?, ReglaDto> grupo)
    {
        var tipo = grupo.FirstOrDefault(r => r.TipoModificador == "size");
        var sabor = grupo.FirstOrDefault(r => r.TipoModificador == "flavor");
        if (grupo.Key?.StartsWith("Hot dog", StringComparison.OrdinalIgnoreCase) != true ||
            grupo.Key.Contains("nachos", StringComparison.OrdinalIgnoreCase) || grupo.Count() != 2 ||
            tipo is null || sabor is null) return grupo;

        // La etapa fija usa una clave de presentación. Las dos selecciones del catálogo conservan sus FK.
        var fija = tipo with { ReglaId = -tipo.ReglaId, Min = 1, Max = 1,
            Opciones = [new(-tipo.ReglaId, "Hot dog individual", 0m, null)], TipoModificador = "compound", Titulo = "TIPO DE HOT DOG" };
        var tieneTamanos = sabor.Opciones.All(o => o.Nombre is "Chico" or "Jumbo");
        var tamano = tieneTamanos ? sabor : tipo;
        var preparacion = tieneTamanos ? tipo : sabor;
        return [fija, tamano with { TipoModificador = "size", Titulo = "SELECCIONA EL TAMAÑO",
            Opciones = tamano.Opciones.OrderBy(o => o.Nombre == "Jumbo").Select(o => o with {
                Nombre = tieneTamanos ? "Hot dog " + o.Nombre.ToLowerInvariant() : "Tamaño incluido" }).ToArray() },
            preparacion with { TipoModificador = "flavor", Titulo = "SELECCIONA LA PREPARACIÓN",
            Opciones = preparacion.Opciones.Select(o => o.Nombre == "Hot Dog" ? o with { Nombre = "Hot dog tradicional" } : o).ToArray() }];
    }

    private static readonly string[] Bebidas = ["refresco", "bebida", "icee", "agua", "frapp", "cerveza", "café", "capuccino"];

    private static IEnumerable<ReglaDto> NumerarComponentes(ReglaDto[] reglas)
    {
        var nombres = new Dictionary<(string? Original, int? Bloque), string>();
        var componentes = reglas.Select(r => (r.GrupoTitulo, r.BloquePalomitas)).Distinct();
        foreach (var familia in componentes.GroupBy(c => NormalizadorEtiquetasPasoHandler.Nombre(c.GrupoTitulo)))
        {
            var unidades = familia.ToArray();
            for (var i = 0; i < unidades.Length; i++)
                nombres[unidades[i]] = unidades.Length == 1 ? familia.Key : $"{familia.Key} {i + 1}";
        }
        return reglas.Select(r => r with { GrupoTitulo = nombres[(r.GrupoTitulo, r.BloquePalomitas)] });
    }

    internal static bool EsBebida(ReglaDto regla) =>
        Bebidas.Any(b => regla.GrupoTitulo?.Contains(b, StringComparison.OrdinalIgnoreCase) == true);

    private static int OrdenComponente(ReglaDto regla)
    {
        if (regla.BloquePalomitas.HasValue) return 0;
        return EsBebida(regla) ? 1 : 2;
    }
}
