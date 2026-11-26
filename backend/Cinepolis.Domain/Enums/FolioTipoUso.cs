namespace Cinepolis.Domain;

public enum FolioTipoUso
{
    Boletos,
    Alimentos
}

public static class ValoresFolioTipoUso
{
    public static FolioTipoUso DesdeTexto(string valor) => valor switch
    {
        "boletos" => FolioTipoUso.Boletos,
        "alimentos" => FolioTipoUso.Alimentos,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };

    public static string TextoSql(this FolioTipoUso valor) => valor switch
    {
        FolioTipoUso.Boletos => "boletos",
        FolioTipoUso.Alimentos => "alimentos",
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };
}
