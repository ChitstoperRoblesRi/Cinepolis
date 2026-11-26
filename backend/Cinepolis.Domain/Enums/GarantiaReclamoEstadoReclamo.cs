namespace Cinepolis.Domain;

public enum GarantiaReclamoEstadoReclamo
{
    Pendiente,
    Aprobado,
    Rechazado
}

public static class ValoresGarantiaReclamoEstadoReclamo
{
    public static GarantiaReclamoEstadoReclamo DesdeTexto(string valor) => valor switch
    {
        "pendiente" => GarantiaReclamoEstadoReclamo.Pendiente,
        "aprobado" => GarantiaReclamoEstadoReclamo.Aprobado,
        "rechazado" => GarantiaReclamoEstadoReclamo.Rechazado,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };

    public static string TextoSql(this GarantiaReclamoEstadoReclamo valor) => valor switch
    {
        GarantiaReclamoEstadoReclamo.Pendiente => "pendiente",
        GarantiaReclamoEstadoReclamo.Aprobado => "aprobado",
        GarantiaReclamoEstadoReclamo.Rechazado => "rechazado",
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };
}
