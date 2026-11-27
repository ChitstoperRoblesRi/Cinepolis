namespace Cinepolis.Domain;

/// <summary>Valores admitidos para venta forma pago.</summary>
public enum VentaFormaPago
{
    Tarjeta,
    PayPal,
    ClickToPay,
    Aplazo,
    Efectivo,
    Puntos
}

/// <summary>Convierte entre el valor tipado y su representación en PostgreSQL.</summary>
public static class ValoresVentaFormaPago
{
    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public static VentaFormaPago DesdeTexto(string valor) => valor switch
    {
        "Tarjeta" => VentaFormaPago.Tarjeta,
        "PayPal" => VentaFormaPago.PayPal,
        "ClickToPay" => VentaFormaPago.ClickToPay,
        "Aplazo" => VentaFormaPago.Aplazo,
        "Efectivo" => VentaFormaPago.Efectivo,
        "Puntos" => VentaFormaPago.Puntos,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };

    /// <summary>Devuelve la representación persistida del valor de dominio.</summary>
    public static string TextoSql(this VentaFormaPago valor) => valor switch
    {
        VentaFormaPago.Tarjeta => "Tarjeta",
        VentaFormaPago.PayPal => "PayPal",
        VentaFormaPago.ClickToPay => "ClickToPay",
        VentaFormaPago.Aplazo => "Aplazo",
        VentaFormaPago.Efectivo => "Efectivo",
        VentaFormaPago.Puntos => "Puntos",
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };
}
