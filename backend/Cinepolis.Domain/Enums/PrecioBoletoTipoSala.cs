namespace Cinepolis.Domain;

/// <summary>Valores admitidos para precio boleto tipo sala.</summary>
public enum PrecioBoletoTipoSala
{
    Tradicional,
    MACROXE,
    IMAX,
    VIP,
    Junior,
    Valor4DX
}

/// <summary>Convierte entre el valor tipado y su representación en PostgreSQL.</summary>
public static class ValoresPrecioBoletoTipoSala
{
    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public static PrecioBoletoTipoSala DesdeTexto(string valor) => valor switch
    {
        "Tradicional" => PrecioBoletoTipoSala.Tradicional,
        "MACRO XE" => PrecioBoletoTipoSala.MACROXE,
        "IMAX" => PrecioBoletoTipoSala.IMAX,
        "VIP" => PrecioBoletoTipoSala.VIP,
        "Junior" => PrecioBoletoTipoSala.Junior,
        "4DX" => PrecioBoletoTipoSala.Valor4DX,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };

    /// <summary>Devuelve la representación persistida del valor de dominio.</summary>
    public static string TextoSql(this PrecioBoletoTipoSala valor) => valor switch
    {
        PrecioBoletoTipoSala.Tradicional => "Tradicional",
        PrecioBoletoTipoSala.MACROXE => "MACRO XE",
        PrecioBoletoTipoSala.IMAX => "IMAX",
        PrecioBoletoTipoSala.VIP => "VIP",
        PrecioBoletoTipoSala.Junior => "Junior",
        PrecioBoletoTipoSala.Valor4DX => "4DX",
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };
}
