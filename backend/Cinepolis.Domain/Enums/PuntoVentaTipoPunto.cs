namespace Cinepolis.Domain;

/// <summary>Valores admitidos para punto venta tipo punto.</summary>
public enum PuntoVentaTipoPunto
{
    Taquilla,
    Kiosco,
    Dulcería,
    Online
}

/// <summary>Convierte entre el valor tipado y su representación en PostgreSQL.</summary>
public static class ValoresPuntoVentaTipoPunto
{
    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public static PuntoVentaTipoPunto DesdeTexto(string valor) => valor switch
    {
        "Taquilla" => PuntoVentaTipoPunto.Taquilla,
        "Kiosco" => PuntoVentaTipoPunto.Kiosco,
        "Dulcería" => PuntoVentaTipoPunto.Dulcería,
        "Online" => PuntoVentaTipoPunto.Online,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };

    /// <summary>Devuelve la representación persistida del valor de dominio.</summary>
    public static string TextoSql(this PuntoVentaTipoPunto valor) => valor switch
    {
        PuntoVentaTipoPunto.Taquilla => "Taquilla",
        PuntoVentaTipoPunto.Kiosco => "Kiosco",
        PuntoVentaTipoPunto.Dulcería => "Dulcería",
        PuntoVentaTipoPunto.Online => "Online",
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };
}
