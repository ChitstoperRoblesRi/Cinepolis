namespace Cinepolis.Domain;

public enum FolioEstado
{
    Activo,
    Usado,
    Vencido
}

public static class ValoresFolioEstado
{
    public static FolioEstado DesdeTexto(string valor) => valor switch
    {
        "activo" => FolioEstado.Activo,
        "usado" => FolioEstado.Usado,
        "vencido" => FolioEstado.Vencido,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };

    public static string TextoSql(this FolioEstado valor) => valor switch
    {
        FolioEstado.Activo => "activo",
        FolioEstado.Usado => "usado",
        FolioEstado.Vencido => "vencido",
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };
}
