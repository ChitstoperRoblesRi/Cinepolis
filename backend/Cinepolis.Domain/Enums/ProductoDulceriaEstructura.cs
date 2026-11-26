namespace Cinepolis.Domain;

public enum ProductoDulceriaEstructura
{
    Simple,
    Compound,
    Combo
}

public static class ValoresProductoDulceriaEstructura
{
    public static ProductoDulceriaEstructura DesdeTexto(string valor) => valor switch
    {
        "simple" => ProductoDulceriaEstructura.Simple,
        "compound" => ProductoDulceriaEstructura.Compound,
        "combo" => ProductoDulceriaEstructura.Combo,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };

    public static string TextoSql(this ProductoDulceriaEstructura valor) => valor switch
    {
        ProductoDulceriaEstructura.Simple => "simple",
        ProductoDulceriaEstructura.Compound => "compound",
        ProductoDulceriaEstructura.Combo => "combo",
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };
}
