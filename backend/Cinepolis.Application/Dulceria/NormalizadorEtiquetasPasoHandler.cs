using System.Text.RegularExpressions;

namespace Cinepolis.Application;

public sealed class NormalizadorEtiquetasPasoHandler : INormalizadorCatalogoHandler
{
    internal static string Nombre(string? grupo)
    {
        var nombre = Regex.Replace(grupo?.Trim() ?? "", @"^(?:selecciona[r]?|personalizar)\s+(?:(?:el sabor|el tamaño)\s+de\s+)?(?:(?:tu[s]?|el|la[s]?)\s+)?", "", RegexOptions.IgnoreCase);
        nombre = Regex.Replace(nombre, @"(?:\s+(?:n[uú]mero\s+)?\d+)+$", "", RegexOptions.IgnoreCase);
        nombre = Regex.Replace(nombre, @"\s+(?:jumbo|grandes?|para llevar)$", "", RegexOptions.IgnoreCase).Trim();
        return nombre.Length == 0 ? "Complemento" : char.ToUpperInvariant(nombre[0]) + nombre[1..];
    }

    public ProductoDto Normalizar(ProductoDto producto, DatosCatalogo datos) => producto with
    {
        Reglas = producto.Reglas.Select(r => datos.EsCombo ? r with { Titulo = Accion(r) } :
            r.BloquePalomitas.HasValue ? r with { GrupoTitulo = "Palomitas", Titulo = Accion(r) } : r).ToArray()
    };

    private static string Accion(ReglaDto regla)
    {
        if (regla.Titulo is "TIPO DE HOT DOG" or "SELECCIONA EL TAMAÑO" or "SELECCIONA LA PREPARACIÓN") return regla.Titulo;
        if (regla.TipoModificador == "ingredient" && regla.Opciones.Any(o => o.Nombre.Contains("hielo", StringComparison.OrdinalIgnoreCase)))
            return "CANTIDAD DE HIELOS";
        var bebidaOPalomitas = regla.BloquePalomitas.HasValue || NormalizadorCombosHandler.EsBebida(regla);
        if (bebidaOPalomitas && regla.TipoModificador == "size" ||
            regla.Opciones.Length > 0 && regla.Opciones.All(o =>
                Regex.IsMatch(o.Nombre.Trim(), @"^(chic[oa]s?|median[oa]s?|grandes?|jumbo|para llevar)$", RegexOptions.IgnoreCase)))
            return "SELECCIONA EL TAMAÑO";
        if (regla.TipoModificador == "flavor" &&
            !string.Equals(regla.GrupoTitulo, "Producto personalizado", StringComparison.OrdinalIgnoreCase))
            return "SELECCIONA EL SABOR";
        return regla.TipoModificador == "extra" ? "SELECCIONA TUS EXTRAS" : "SELECCIONA TU OPCIÓN";
    }
}
