namespace Cinepolis.Domain;

public enum PrecioBoletoTipoSala
{
    Tradicional,
    MACROXE,
    IMAX,
    VIP,
    Junior,
    Valor4DX
}

public static class ValoresPrecioBoletoTipoSala
{
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
