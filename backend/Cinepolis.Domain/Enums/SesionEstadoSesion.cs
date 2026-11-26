namespace Cinepolis.Domain;

public enum SesionEstadoSesion
{
    Abierta,
    Cerrada
}

public static class ValoresSesionEstadoSesion
{
    public static SesionEstadoSesion DesdeTexto(string valor) => valor switch
    {
        "abierta" => SesionEstadoSesion.Abierta,
        "cerrada" => SesionEstadoSesion.Cerrada,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };

    public static string TextoSql(this SesionEstadoSesion valor) => valor switch
    {
        SesionEstadoSesion.Abierta => "abierta",
        SesionEstadoSesion.Cerrada => "cerrada",
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };
}
