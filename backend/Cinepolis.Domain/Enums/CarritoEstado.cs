namespace Cinepolis.Domain;

public enum CarritoEstado
{
    Activo,
    Abandonado,
    Convertido
}

public static class ValoresCarritoEstado
{
    public static CarritoEstado DesdeTexto(string valor) => valor switch
    {
        "activo" => CarritoEstado.Activo,
        "abandonado" => CarritoEstado.Abandonado,
        "convertido" => CarritoEstado.Convertido,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };

    public static string TextoSql(this CarritoEstado valor) => valor switch
    {
        CarritoEstado.Activo => "activo",
        CarritoEstado.Abandonado => "abandonado",
        CarritoEstado.Convertido => "convertido",
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };
}
