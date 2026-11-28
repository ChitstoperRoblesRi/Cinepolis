using Cinepolis.Domain;

namespace Cinepolis.Application;

/// <summary>Publica el rango efectivo de cada bloque usando los identificadores de tamaño del catálogo.</summary>
public sealed class NormalizadorLimitesSaboresHandler : INormalizadorCatalogoHandler
{
    public ProductoDto Normalizar(ProductoDto producto, DatosCatalogo datos)
    {
        ReglaDto Limites(ReglaDto regla)
        {
            if (!regla.SaboresPalomitas) return regla;
            var tamanos = producto.Reglas.Where(r => r.BloquePalomitas == regla.BloquePalomitas && r.TipoModificador == "size")
                .SelectMany(r => r.Opciones).ToDictionary(o => o.OpcionId,
                    o => o.Nombre.Trim().Equals("Para llevar", StringComparison.OrdinalIgnoreCase) ? 4 : 2);
            return regla with { Min = 1, Max = tamanos.Values.DefaultIfEmpty(Math.Min(regla.Max, 2)).Max(), MaximosPorTamano = tamanos };
        }
        return producto with { Reglas = producto.Reglas.Select(Limites).ToArray() };
    }
}

public interface IValidadorSeleccionDulceria
{
    OpcionDto[] Validar(ProductoDto producto, int[] seleccion);
}

public sealed class ValidadorSeleccionDulceria : IValidadorSeleccionDulceria
{
    public OpcionDto[] Validar(ProductoDto producto, int[] seleccion)
    {
        var ids = seleccion.ToHashSet();
        var opciones = producto.Reglas.SelectMany(r => r.Opciones).Where(o => ids.Contains(o.OpcionId)).ToArray();
        bool FueraDeRango(ReglaDto regla)
        {
            var maximo = regla.MaximosPorTamano?.Where(t => ids.Contains(t.Key)).Select(t => t.Value)
                .DefaultIfEmpty(regla.Max).Min() ?? regla.Max;
            var cantidad = regla.Opciones.Count(o => ids.Contains(o.OpcionId));
            return cantidad < regla.Min || cantidad > maximo;
        }
        if (opciones.Length != seleccion.Length || producto.Reglas.Any(FueraDeRango))
            throw new ConflictoCompra("Revisa las opciones de " + producto.NombreProducto + ".");
        return opciones;
    }
}
