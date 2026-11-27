namespace Cinepolis.Domain;

/// <summary>Valores admitidos para empleado estatus.</summary>
public enum EmpleadoEstatus
{
    Activo,
    Inactivo,
    Baja
}

/// <summary>Convierte entre el valor tipado y su representación en PostgreSQL.</summary>
public static class ValoresEmpleadoEstatus
{
    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public static EmpleadoEstatus DesdeTexto(string valor) => valor switch
    {
        "activo" => EmpleadoEstatus.Activo,
        "inactivo" => EmpleadoEstatus.Inactivo,
        "baja" => EmpleadoEstatus.Baja,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };

    /// <summary>Devuelve la representación persistida del valor de dominio.</summary>
    public static string TextoSql(this EmpleadoEstatus valor) => valor switch
    {
        EmpleadoEstatus.Activo => "activo",
        EmpleadoEstatus.Inactivo => "inactivo",
        EmpleadoEstatus.Baja => "baja",
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };
}
