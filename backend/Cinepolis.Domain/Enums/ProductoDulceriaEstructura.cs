namespace Cinepolis.Domain;

/// <summary>Valores admitidos para producto dulceria estructura.</summary>
public enum ProductoDulceriaEstructura
{
    Simple,
    Compound,
    Combo
}

/// <summary>Convierte entre el valor tipado y su representación en PostgreSQL.</summary>
public static class ValoresProductoDulceriaEstructura
{
    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public static ProductoDulceriaEstructura DesdeTexto(string valor) => valor switch
    {
        "simple" => ProductoDulceriaEstructura.Simple,
        "compound" => ProductoDulceriaEstructura.Compound,
        "combo" => ProductoDulceriaEstructura.Combo,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };

    /// <summary>Devuelve la representación persistida del valor de dominio.</summary>
    public static string TextoSql(this ProductoDulceriaEstructura valor) => valor switch
    {
        ProductoDulceriaEstructura.Simple => "simple",
        ProductoDulceriaEstructura.Compound => "compound",
        ProductoDulceriaEstructura.Combo => "combo",
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };
}
