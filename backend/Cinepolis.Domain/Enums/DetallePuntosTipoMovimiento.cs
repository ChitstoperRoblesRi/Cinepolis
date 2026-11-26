namespace Cinepolis.Domain;

public enum DetallePuntosTipoMovimiento
{
    Acumulacion,
    Canje,
    Vencimiento
}

public static class ValoresDetallePuntosTipoMovimiento
{
    public static DetallePuntosTipoMovimiento DesdeTexto(string valor) => valor switch
    {
        "acumulacion" => DetallePuntosTipoMovimiento.Acumulacion,
        "canje" => DetallePuntosTipoMovimiento.Canje,
        "vencimiento" => DetallePuntosTipoMovimiento.Vencimiento,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };

    public static string TextoSql(this DetallePuntosTipoMovimiento valor) => valor switch
    {
        DetallePuntosTipoMovimiento.Acumulacion => "acumulacion",
        DetallePuntosTipoMovimiento.Canje => "canje",
        DetallePuntosTipoMovimiento.Vencimiento => "vencimiento",
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };
}
