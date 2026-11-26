namespace Cinepolis.Domain;

public enum VentaFormaPago
{
    Tarjeta,
    PayPal,
    ClickToPay,
    Aplazo,
    Efectivo,
    Puntos
}

public static class ValoresVentaFormaPago
{
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
