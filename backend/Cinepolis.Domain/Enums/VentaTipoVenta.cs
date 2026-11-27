namespace Cinepolis.Domain;

/// <summary>Valores admitidos para venta tipo venta.</summary>
public enum VentaTipoVenta
{
    Online,
    Taquilla,
    Kiosco,
    Dulcería
}

/// <summary>Convierte entre el valor tipado y su representación en PostgreSQL.</summary>
public static class ValoresVentaTipoVenta
{
    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public static VentaTipoVenta DesdeTexto(string valor) => valor switch
    {
        "Online" => VentaTipoVenta.Online,
        "Taquilla" => VentaTipoVenta.Taquilla,
        "Kiosco" => VentaTipoVenta.Kiosco,
        "Dulcería" => VentaTipoVenta.Dulcería,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };

    /// <summary>Devuelve la representación persistida del valor de dominio.</summary>
    public static string TextoSql(this VentaTipoVenta valor) => valor switch
    {
        VentaTipoVenta.Online => "Online",
        VentaTipoVenta.Taquilla => "Taquilla",
        VentaTipoVenta.Kiosco => "Kiosco",
        VentaTipoVenta.Dulcería => "Dulcería",
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };
}
