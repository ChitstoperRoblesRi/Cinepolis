namespace Cinepolis.Domain;

/// <summary>Valores admitidos para carrito estado.</summary>
public enum CarritoEstado
{
    Activo,
    Abandonado,
    Convertido
}

/// <summary>Convierte entre el valor tipado y su representación en PostgreSQL.</summary>
public static class ValoresCarritoEstado
{
    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public static CarritoEstado DesdeTexto(string valor) => valor switch
    {
        "activo" => CarritoEstado.Activo,
        "abandonado" => CarritoEstado.Abandonado,
        "convertido" => CarritoEstado.Convertido,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };

    /// <summary>Devuelve la representación persistida del valor de dominio.</summary>
    public static string TextoSql(this CarritoEstado valor) => valor switch
    {
        CarritoEstado.Activo => "activo",
        CarritoEstado.Abandonado => "abandonado",
        CarritoEstado.Convertido => "convertido",
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };
}
