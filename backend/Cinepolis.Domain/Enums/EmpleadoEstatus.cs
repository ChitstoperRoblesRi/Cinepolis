namespace Cinepolis.Domain;

public enum EmpleadoEstatus
{
    Activo,
    Inactivo,
    Baja
}

public static class ValoresEmpleadoEstatus
{
    public static EmpleadoEstatus DesdeTexto(string valor) => valor switch
    {
        "activo" => EmpleadoEstatus.Activo,
        "inactivo" => EmpleadoEstatus.Inactivo,
        "baja" => EmpleadoEstatus.Baja,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };

    public static string TextoSql(this EmpleadoEstatus valor) => valor switch
    {
        EmpleadoEstatus.Activo => "activo",
        EmpleadoEstatus.Inactivo => "inactivo",
        EmpleadoEstatus.Baja => "baja",
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };
}
