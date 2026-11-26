namespace Cinepolis.Domain;

public enum PuntoVentaTipoPunto
{
    Taquilla,
    Kiosco,
    Dulcería,
    Online
}

public static class ValoresPuntoVentaTipoPunto
{
    public static PuntoVentaTipoPunto DesdeTexto(string valor) => valor switch
    {
        "Taquilla" => PuntoVentaTipoPunto.Taquilla,
        "Kiosco" => PuntoVentaTipoPunto.Kiosco,
        "Dulcería" => PuntoVentaTipoPunto.Dulcería,
        "Online" => PuntoVentaTipoPunto.Online,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };

    public static string TextoSql(this PuntoVentaTipoPunto valor) => valor switch
    {
        PuntoVentaTipoPunto.Taquilla => "Taquilla",
        PuntoVentaTipoPunto.Kiosco => "Kiosco",
        PuntoVentaTipoPunto.Dulcería => "Dulcería",
        PuntoVentaTipoPunto.Online => "Online",
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };
}
