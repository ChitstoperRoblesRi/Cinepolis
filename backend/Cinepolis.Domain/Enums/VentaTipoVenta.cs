namespace Cinepolis.Domain;

public enum VentaTipoVenta
{
    Online,
    Taquilla,
    Kiosco,
    Dulcería
}

public static class ValoresVentaTipoVenta
{
    public static VentaTipoVenta DesdeTexto(string valor) => valor switch
    {
        "Online" => VentaTipoVenta.Online,
        "Taquilla" => VentaTipoVenta.Taquilla,
        "Kiosco" => VentaTipoVenta.Kiosco,
        "Dulcería" => VentaTipoVenta.Dulcería,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };

    public static string TextoSql(this VentaTipoVenta valor) => valor switch
    {
        VentaTipoVenta.Online => "Online",
        VentaTipoVenta.Taquilla => "Taquilla",
        VentaTipoVenta.Kiosco => "Kiosco",
        VentaTipoVenta.Dulcería => "Dulcería",
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };
}
