namespace Cinepolis.Domain;

/// <summary>Valores admitidos para sesion estado sesion.</summary>
public enum SesionEstadoSesion
{
    Abierta,
    Cerrada
}

/// <summary>Convierte entre el valor tipado y su representación en PostgreSQL.</summary>
public static class ValoresSesionEstadoSesion
{
    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public static SesionEstadoSesion DesdeTexto(string valor) => valor switch
    {
        "abierta" => SesionEstadoSesion.Abierta,
        "cerrada" => SesionEstadoSesion.Cerrada,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };

    /// <summary>Devuelve la representación persistida del valor de dominio.</summary>
    public static string TextoSql(this SesionEstadoSesion valor) => valor switch
    {
        SesionEstadoSesion.Abierta => "abierta",
        SesionEstadoSesion.Cerrada => "cerrada",
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };
}
