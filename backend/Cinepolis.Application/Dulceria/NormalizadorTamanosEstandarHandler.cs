using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Cinepolis.Application;

/// <summary>Normaliza presentaciones genéricas y ordena tamaños, capacidades y porciones.</summary>
public sealed class NormalizadorTamanosEstandarHandler : INormalizadorCatalogoHandler
{
    /// <inheritdoc/>
    public ProductoDto Normalizar(ProductoDto p, DatosCatalogo datos) =>
        p with { Reglas = p.Reglas.Select(r => NormalizarTamanos(p, r)).ToArray() };

    private static string Clave(string texto) => string.Concat(texto.Trim().ToLowerInvariant()
        .Normalize(NormalizationForm.FormD).Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark));

    private static int Prioridad(string nombre) => Clave(nombre) switch
    {
        "jumbo" => 1,
        "grande" or "grandes" => 2,
        "mediano" or "mediana" or "medianos" or "medianas" => 3,
        "chico" or "chica" or "chicos" or "chicas" => 4,
        _ => 99
    };

    private static readonly string[] Tamanos = ["Jumbo", "Grande", "Mediano", "Chico"];

    private static (int Tipo, decimal Cantidad) Medida(string nombre)
    {
        var m = Regex.Match(Clave(nombre),
            @"(\d+(?:[.,]\d+)?)\s*(ml|mililitros?|l|lt|lts|litros?|oz|onzas?|g|gr|gramos?|kg|pz|pzas?|piezas?)\b");
        if (!m.Success || !decimal.TryParse(m.Groups[1].Value.Replace(',', '.'), NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out var cantidad))
            return (99, 0);
        return m.Groups[2].Value switch
        {
            "ml" or "mililitro" or "mililitros" => (1, cantidad),
            "oz" or "onza" or "onzas" => (1, cantidad * 29.5735295625m),
            "g" or "gr" or "gramo" or "gramos" => (2, cantidad),
            "kg" => (2, cantidad * 1000),
            var unidad when unidad.StartsWith('p') => (3, cantidad),
            _ => (1, cantidad * 1000)
        };
    }

    private static ReglaDto NormalizarTamanos(ProductoDto producto, ReglaDto regla)
    {
        if (regla.TipoModificador == "size" && (producto.NombreProducto == "Nachos" ||
            Clave(regla.GrupoTitulo ?? "").Contains("nachos") && regla.Opciones.Any(o => o.Nombre == "Grandes")))
        {
            var tamanos = regla.Opciones.Select(o => Prioridad(o.Nombre) switch
            {
                2 => o with { Nombre = "Grandes" },
                4 => o with { Nombre = "Chicos" },
                _ => o
            });
            return regla with { Opciones = tamanos.OrderBy(o => Prioridad(o.Nombre)).ToArray() };
        }
        var titulo = Clave(regla.Titulo);
        var grupo = Clave(regla.GrupoTitulo ?? "");
        static bool EsPresentacion(string texto) => texto.Contains("tamano") || texto.Contains("presentacion");
        if (!EsPresentacion(titulo) && (titulo.Contains("sabor") ||
            !(EsPresentacion(grupo) || regla.TipoModificador.Equals("size", StringComparison.OrdinalIgnoreCase))))
            return regla;

        var baseProducto = Clave(producto.NombreProducto);
        decimal Precio(OpcionDto o) => o.PrecioVariante ?? (producto.Precio ?? 0) + o.Extra;
        var conocidos = regla.Opciones.Where(o => Prioridad(o.Nombre) <= Tamanos.Length).ToArray();
        var genericas = regla.Opciones.Where(o => Clave(o.Nombre) == baseProducto).ToArray();
        var opciones = regla.Opciones.ToArray();
        if (genericas.Length == 1 && regla.Opciones.All(o => Medida(o.Nombre).Tipo == 99))
        {
            var generica = genericas[0];
            var disponibles = Enumerable.Range(1, Tamanos.Length).Where(rango => conocidos.All(o =>
                Prioridad(o.Nombre) != rango && (Precio(o) == Precio(generica) ||
                (Precio(o) > Precio(generica)) == (Prioridad(o.Nombre) < rango)))).ToArray();
            // Con dos presentaciones, la inmediatamente inferior a Jumbo es Grande.
            var rangoElegido = disponibles.Length == 1 ? disponibles[0] :
                conocidos.Any(o => Prioridad(o.Nombre) == 1 && Precio(o) > Precio(generica))
                    && disponibles.Contains(2) ? 2 : 0;
            if (rangoElegido > 0)
                opciones[Array.IndexOf(opciones, generica)] = generica with { Nombre = Tamanos[rangoElegido - 1] };
        }
        opciones = opciones.Select(o => (Opcion: o, Rango: Prioridad(o.Nombre), Medida: Medida(o.Nombre)))
            .OrderByDescending(x => regla.BloquePalomitas.HasValue && producto.Estructura == "combo" && baseProducto != "palomitas" &&
                Clave(x.Opcion.Nombre) == "para llevar")
            .ThenBy(x => x.Rango).ThenBy(x => x.Medida.Tipo).ThenByDescending(x => x.Medida.Cantidad)
            .ThenByDescending(x => Precio(x.Opcion)).ThenBy(x => x.Opcion.OpcionId)
            .Select(x => x.Rango <= Tamanos.Length ? x.Opcion with { Nombre = Tamanos[x.Rango - 1] } : x.Opcion)
            .ToArray();
        return regla with { Opciones = opciones };
    }
}
