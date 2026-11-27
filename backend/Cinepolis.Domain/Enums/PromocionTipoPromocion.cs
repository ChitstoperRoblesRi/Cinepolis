namespace Cinepolis.Domain;

/// <summary>Valores admitidos para promocion tipo promocion.</summary>
public enum PromocionTipoPromocion
{
    Descuento,
    Valor2x1,
    Combo,
    BeneficioNivel,
    PrecioEspecial
}

/// <summary>Convierte entre el valor tipado y su representación en PostgreSQL.</summary>
public static class ValoresPromocionTipoPromocion
{
    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public static PromocionTipoPromocion DesdeTexto(string valor) => valor switch
    {
        "descuento" => PromocionTipoPromocion.Descuento,
        "2x1" => PromocionTipoPromocion.Valor2x1,
        "combo" => PromocionTipoPromocion.Combo,
        "beneficio_nivel" => PromocionTipoPromocion.BeneficioNivel,
        "precio_especial" => PromocionTipoPromocion.PrecioEspecial,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };

    /// <summary>Devuelve la representación persistida del valor de dominio.</summary>
    public static string TextoSql(this PromocionTipoPromocion valor) => valor switch
    {
        PromocionTipoPromocion.Descuento => "descuento",
        PromocionTipoPromocion.Valor2x1 => "2x1",
        PromocionTipoPromocion.Combo => "combo",
        PromocionTipoPromocion.BeneficioNivel => "beneficio_nivel",
        PromocionTipoPromocion.PrecioEspecial => "precio_especial",
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };
}
