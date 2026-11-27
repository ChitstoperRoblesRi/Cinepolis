namespace Cinepolis.Domain;

/// <summary>Importes y referencias del catálogo regional para una opción de personalización.</summary>
public sealed record PrecioOpcionCatalogo(decimal Extra, decimal? PrecioCatalogo, decimal? BaseBebida,
    decimal? SkuPrincipal, decimal? ParaLlevar, decimal? Incluido, decimal? Minimo, bool EleccionUnica,
    bool AguaEnCombo, bool TamanoNachos, bool TamanoPalomitasCombo, string NombreOrigen, bool OpcionIncluidaCombo,
    bool VarianteHelado);

/// <summary>Diferenciales de un grupo seleccionado sobre el precio de portada o variante.</summary>
public sealed record GrupoPrecio(bool Combo, bool SaboresPalomitas, IReadOnlyList<decimal> Extras);

/// <summary>Regla de cobro para opciones incluidas en una misma selección.</summary>
public interface IPrecioDulceriaStrategy
{
    /// <summary>Reconoce la clase de componente que puede valorar.</summary>
    bool Aplica(GrupoPrecio grupo);
    /// <summary>Calcula el ajuste sobre el precio unitario.</summary>
    decimal Diferencial(GrupoPrecio grupo);
    /// <summary>Convierte un importe de origen en el diferencial de la opción.</summary>
    decimal Ajustar(PrecioOpcionCatalogo opcion);
}

/// <summary>Suma extras de productos simples sobre su presentación elegida.</summary>
public sealed class EstrategiaPrecioProductoBase : IPrecioDulceriaStrategy
{
    /// <summary>Descuenta los importes incluidos en la presentación base o en la variante directa.</summary>
    public decimal Ajustar(PrecioOpcionCatalogo o)
    {
        if (o.TamanoNachos && o.SkuPrincipal.HasValue) return o.Extra - o.SkuPrincipal.Value;
        if (o.Extra >= o.BaseBebida) return o.Extra - o.BaseBebida!.Value;
        if (o.EleccionUnica && o.Extra >= o.PrecioCatalogo) return o.Extra - o.PrecioCatalogo!.Value;
        if (o.VarianteHelado && o.Minimo.HasValue) return o.Extra - o.Minimo.Value;
        return o.Extra;
    }

    /// <inheritdoc/>
    public bool Aplica(GrupoPrecio grupo) => !grupo.Combo && !grupo.SaboresPalomitas;
    /// <inheritdoc/>
    public decimal Diferencial(GrupoPrecio grupo) => grupo.Extras.Sum();
}

/// <summary>Cobra una sola vez el sabor más caro por bloque de palomitas.</summary>
public sealed class EstrategiaSaboresPalomitasMaxExtra : IPrecioDulceriaStrategy
{
    /// <inheritdoc/>
    public decimal Ajustar(PrecioOpcionCatalogo opcion) => opcion.Extra;
    /// <inheritdoc/>
    public bool Aplica(GrupoPrecio grupo) => grupo.SaboresPalomitas;
    /// <inheritdoc/>
    public decimal Diferencial(GrupoPrecio grupo) => grupo.Extras.DefaultIfEmpty().Max();
}

/// <summary>Aplica los diferenciales firmados del catálogo, incluidos descuentos e inclusiones.</summary>
public sealed class EstrategiaPrecioCombo(EstrategiaPrecioProductoBase precioBase) : IPrecioDulceriaStrategy
{
    /// <summary>Calcula los ajustes sobre la portada Para llevar y las bebidas incluidas.</summary>
    public decimal Ajustar(PrecioOpcionCatalogo o)
    {
        if (o.AguaEnCombo) return o.Extra >= o.BaseBebida ? o.Extra - o.BaseBebida!.Value : o.Extra;
        if (o.TamanoPalomitasCombo && o.ParaLlevar is decimal portada)
        {
            var nombre = o.NombreOrigen.Trim().ToLowerInvariant();
            if (nombre == "para llevar") return 0m;
            if (nombre is "jumbo" or "palomitas") return -Math.Abs(o.Extra - portada);
        }
        if (o.OpcionIncluidaCombo && o.Minimo > 0 && o.Incluido > 0)
            return Math.Max(0m, o.Extra - o.Incluido.Value);
        return precioBase.Ajustar(o);
    }

    /// <inheritdoc/>
    public bool Aplica(GrupoPrecio grupo) => grupo.Combo && !grupo.SaboresPalomitas;
    /// <inheritdoc/>
    public decimal Diferencial(GrupoPrecio grupo) => grupo.Extras.Sum();
}
