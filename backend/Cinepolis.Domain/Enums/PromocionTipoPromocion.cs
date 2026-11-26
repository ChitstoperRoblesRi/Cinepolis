namespace Cinepolis.Domain;

public enum PromocionTipoPromocion
{
    Descuento,
    Valor2x1,
    Combo,
    BeneficioNivel,
    PrecioEspecial
}

public static class ValoresPromocionTipoPromocion
{
    public static PromocionTipoPromocion DesdeTexto(string valor) => valor switch
    {
        "descuento" => PromocionTipoPromocion.Descuento,
        "2x1" => PromocionTipoPromocion.Valor2x1,
        "combo" => PromocionTipoPromocion.Combo,
        "beneficio_nivel" => PromocionTipoPromocion.BeneficioNivel,
        "precio_especial" => PromocionTipoPromocion.PrecioEspecial,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };

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
