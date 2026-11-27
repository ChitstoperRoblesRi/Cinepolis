namespace Cinepolis.Domain;

/// <summary>Valores admitidos para asiento estado.</summary>
public enum AsientoEstado
{
    Disponible,
    Reservado,
    Vendido,
    Bloqueado
}

/// <summary>Convierte entre el valor tipado y su representación en PostgreSQL.</summary>
public static class ValoresAsientoEstado
{
    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public static AsientoEstado DesdeTexto(string valor) => valor switch
    {
        "disponible" => AsientoEstado.Disponible,
        "reservado" => AsientoEstado.Reservado,
        "vendido" => AsientoEstado.Vendido,
        "bloqueado" => AsientoEstado.Bloqueado,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };

    /// <summary>Devuelve la representación persistida del valor de dominio.</summary>
    public static string TextoSql(this AsientoEstado valor) => valor switch
    {
        AsientoEstado.Disponible => "disponible",
        AsientoEstado.Reservado => "reservado",
        AsientoEstado.Vendido => "vendido",
        AsientoEstado.Bloqueado => "bloqueado",
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };
}
