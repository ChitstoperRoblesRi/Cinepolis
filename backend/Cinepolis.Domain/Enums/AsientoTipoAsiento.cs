namespace Cinepolis.Domain;

public enum AsientoTipoAsiento
{
    Tradicional,
    Especial,
    MACROXE,
    VIP,
    IMAX,
    SillaDeRuedas
}

public static class ValoresAsientoTipoAsiento
{
    public static AsientoTipoAsiento DesdeTexto(string valor) => valor switch
    {
        "Tradicional" => AsientoTipoAsiento.Tradicional,
        "Especial" => AsientoTipoAsiento.Especial,
        "MACRO XE" => AsientoTipoAsiento.MACROXE,
        "VIP" => AsientoTipoAsiento.VIP,
        "IMAX" => AsientoTipoAsiento.IMAX,
        "Silla de ruedas" => AsientoTipoAsiento.SillaDeRuedas,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };

    public static string TextoSql(this AsientoTipoAsiento valor) => valor switch
    {
        AsientoTipoAsiento.Tradicional => "Tradicional",
        AsientoTipoAsiento.Especial => "Especial",
        AsientoTipoAsiento.MACROXE => "MACRO XE",
        AsientoTipoAsiento.VIP => "VIP",
        AsientoTipoAsiento.IMAX => "IMAX",
        AsientoTipoAsiento.SillaDeRuedas => "Silla de ruedas",
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };
}
