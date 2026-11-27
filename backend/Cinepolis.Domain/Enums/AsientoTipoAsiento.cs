namespace Cinepolis.Domain;

/// <summary>Valores admitidos para asiento tipo asiento.</summary>
public enum AsientoTipoAsiento
{
    Tradicional,
    Especial,
    MACROXE,
    VIP,
    IMAX,
    SillaDeRuedas
}

/// <summary>Convierte entre el valor tipado y su representación en PostgreSQL.</summary>
public static class ValoresAsientoTipoAsiento
{
    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
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

    /// <summary>Devuelve la representación persistida del valor de dominio.</summary>
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
