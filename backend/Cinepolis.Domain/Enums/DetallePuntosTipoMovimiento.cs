namespace Cinepolis.Domain;

/// <summary>Valores admitidos para detalle puntos tipo movimiento.</summary>
public enum DetallePuntosTipoMovimiento
{
    Acumulacion,
    Canje,
    Vencimiento
}

/// <summary>Convierte entre el valor tipado y su representación en PostgreSQL.</summary>
public static class ValoresDetallePuntosTipoMovimiento
{
    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public static DetallePuntosTipoMovimiento DesdeTexto(string valor) => valor switch
    {
        "acumulacion" => DetallePuntosTipoMovimiento.Acumulacion,
        "canje" => DetallePuntosTipoMovimiento.Canje,
        "vencimiento" => DetallePuntosTipoMovimiento.Vencimiento,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };

    /// <summary>Devuelve la representación persistida del valor de dominio.</summary>
    public static string TextoSql(this DetallePuntosTipoMovimiento valor) => valor switch
    {
        DetallePuntosTipoMovimiento.Acumulacion => "acumulacion",
        DetallePuntosTipoMovimiento.Canje => "canje",
        DetallePuntosTipoMovimiento.Vencimiento => "vencimiento",
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };
}
