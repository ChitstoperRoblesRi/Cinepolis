namespace Cinepolis.Domain;

public enum DetalleCarritoTipoItem
{
    Boleto,
    Producto
}

public static class ValoresDetalleCarritoTipoItem
{
    public static DetalleCarritoTipoItem DesdeTexto(string valor) => valor switch
    {
        "boleto" => DetalleCarritoTipoItem.Boleto,
        "producto" => DetalleCarritoTipoItem.Producto,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };

    public static string TextoSql(this DetalleCarritoTipoItem valor) => valor switch
    {
        DetalleCarritoTipoItem.Boleto => "boleto",
        DetalleCarritoTipoItem.Producto => "producto",
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };
}
