using Cinepolis.Domain;

namespace Cinepolis.Application;

public interface ICotizadorDulceria
{
    decimal Calcular(ProductoDto producto, IReadOnlyList<OpcionDto> seleccion);
}

/// <summary>Valora cada componente con la política correspondiente a su grupo.</summary>
public sealed class CalculadoraPrecioDulceriaContext(IEnumerable<IPrecioDulceriaStrategy> estrategias) : ICotizadorDulceria
{
    /// <summary>Normaliza el diferencial antes de presentarlo al cliente o usarlo en una compra.</summary>
    public decimal Diferencial(PrecioOpcionCatalogo opcion)
    {
        var combo = opcion.TamanoPalomitasCombo || opcion.OpcionIncluidaCombo;
        return Resolver(new GrupoPrecio(combo, false, [])).Ajustar(opcion);
    }

    private IPrecioDulceriaStrategy Resolver(GrupoPrecio grupo) => estrategias.Single(e => e.Aplica(grupo));

    /// <summary>Parte del precio de variante o portada y aplica únicamente ajustes pendientes.</summary>
    public decimal Calcular(ProductoDto producto, IReadOnlyList<OpcionDto> seleccion)
    {
        var variante = seleccion.SingleOrDefault(o => o.PrecioVariante.HasValue);
        var precio = variante?.PrecioVariante ?? producto.Precio!.Value;
        var ids = seleccion.Where(o => !o.PrecioVariante.HasValue).Select(o => o.OpcionId).ToHashSet();
        foreach (var regla in producto.Reglas)
        {
            var grupo = new GrupoPrecio(producto.Estructura == "combo",
                regla.SaboresPalomitas,
                regla.Opciones.Where(o => ids.Contains(o.OpcionId)).Select(o => o.Extra).ToArray());
            precio += Resolver(grupo).Diferencial(grupo);
        }
        return precio;
    }
}
