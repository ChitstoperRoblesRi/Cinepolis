namespace Cinepolis.Domain;

public enum AsientoEstado
{
    Disponible,
    Reservado,
    Vendido,
    Bloqueado
}

public static class ValoresAsientoEstado
{
    public static AsientoEstado DesdeTexto(string valor) => valor switch
    {
        "disponible" => AsientoEstado.Disponible,
        "reservado" => AsientoEstado.Reservado,
        "vendido" => AsientoEstado.Vendido,
        "bloqueado" => AsientoEstado.Bloqueado,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };

    public static string TextoSql(this AsientoEstado valor) => valor switch
    {
        AsientoEstado.Disponible => "disponible",
        AsientoEstado.Reservado => "reservado",
        AsientoEstado.Vendido => "vendido",
        AsientoEstado.Bloqueado => "bloqueado",
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };
}
