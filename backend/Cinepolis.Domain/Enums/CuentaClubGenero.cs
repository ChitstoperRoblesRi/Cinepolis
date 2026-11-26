namespace Cinepolis.Domain;

public enum CuentaClubGenero
{
    Hombre,
    Mujer,
    PrefieroNoResponder
}

public static class ValoresCuentaClubGenero
{
    public static CuentaClubGenero DesdeTexto(string valor) => valor switch
    {
        "Hombre" => CuentaClubGenero.Hombre,
        "Mujer" => CuentaClubGenero.Mujer,
        "Prefiero no responder" => CuentaClubGenero.PrefieroNoResponder,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };

    public static string TextoSql(this CuentaClubGenero valor) => valor switch
    {
        CuentaClubGenero.Hombre => "Hombre",
        CuentaClubGenero.Mujer => "Mujer",
        CuentaClubGenero.PrefieroNoResponder => "Prefiero no responder",
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };
}
