namespace Cinepolis.Domain;

/// <summary>Valores admitidos para garantia reclamo estado reclamo.</summary>
public enum GarantiaReclamoEstadoReclamo
{
    Pendiente,
    Aprobado,
    Rechazado
}

/// <summary>Convierte entre el valor tipado y su representación en PostgreSQL.</summary>
public static class ValoresGarantiaReclamoEstadoReclamo
{
    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public static GarantiaReclamoEstadoReclamo DesdeTexto(string valor) => valor switch
    {
        "pendiente" => GarantiaReclamoEstadoReclamo.Pendiente,
        "aprobado" => GarantiaReclamoEstadoReclamo.Aprobado,
        "rechazado" => GarantiaReclamoEstadoReclamo.Rechazado,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };

    /// <summary>Devuelve la representación persistida del valor de dominio.</summary>
    public static string TextoSql(this GarantiaReclamoEstadoReclamo valor) => valor switch
    {
        GarantiaReclamoEstadoReclamo.Pendiente => "pendiente",
        GarantiaReclamoEstadoReclamo.Aprobado => "aprobado",
        GarantiaReclamoEstadoReclamo.Rechazado => "rechazado",
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };
}
