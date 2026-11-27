namespace Cinepolis.Domain;

/// <summary>Valores admitidos para cuenta club genero.</summary>
public enum CuentaClubGenero
{
    Hombre,
    Mujer,
    PrefieroNoResponder
}

/// <summary>Convierte entre el valor tipado y su representación en PostgreSQL.</summary>
public static class ValoresCuentaClubGenero
{
    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public static CuentaClubGenero DesdeTexto(string valor) => valor switch
    {
        "Hombre" => CuentaClubGenero.Hombre,
        "Mujer" => CuentaClubGenero.Mujer,
        "Prefiero no responder" => CuentaClubGenero.PrefieroNoResponder,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };

    /// <summary>Devuelve la representación persistida del valor de dominio.</summary>
    public static string TextoSql(this CuentaClubGenero valor) => valor switch
    {
        CuentaClubGenero.Hombre => "Hombre",
        CuentaClubGenero.Mujer => "Mujer",
        CuentaClubGenero.PrefieroNoResponder => "Prefiero no responder",
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };
}
