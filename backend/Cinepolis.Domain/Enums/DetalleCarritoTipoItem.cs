namespace Cinepolis.Domain;

/// <summary>Valores admitidos para detalle carrito tipo item.</summary>
public enum DetalleCarritoTipoItem
{
    Boleto,
    Producto
}

/// <summary>Convierte entre el valor tipado y su representación en PostgreSQL.</summary>
public static class ValoresDetalleCarritoTipoItem
{
    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public static DetalleCarritoTipoItem DesdeTexto(string valor) => valor switch
    {
        "boleto" => DetalleCarritoTipoItem.Boleto,
        "producto" => DetalleCarritoTipoItem.Producto,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };

    /// <summary>Devuelve la representación persistida del valor de dominio.</summary>
    public static string TextoSql(this DetalleCarritoTipoItem valor) => valor switch
    {
        DetalleCarritoTipoItem.Boleto => "boleto",
        DetalleCarritoTipoItem.Producto => "producto",
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };
}
