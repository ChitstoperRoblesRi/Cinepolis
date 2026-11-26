namespace Cinepolis.Domain;

public enum FolioTipoFolio
{
    Corporativo,
    Mastercard2x1,
    Cinepass,
    Promocional
}

public static class ValoresFolioTipoFolio
{
    public static FolioTipoFolio DesdeTexto(string valor) => valor switch
    {
        "Corporativo" => FolioTipoFolio.Corporativo,
        "Mastercard2x1" => FolioTipoFolio.Mastercard2x1,
        "Cinepass" => FolioTipoFolio.Cinepass,
        "Promocional" => FolioTipoFolio.Promocional,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };

    public static string TextoSql(this FolioTipoFolio valor) => valor switch
    {
        FolioTipoFolio.Corporativo => "Corporativo",
        FolioTipoFolio.Mastercard2x1 => "Mastercard2x1",
        FolioTipoFolio.Cinepass => "Cinepass",
        FolioTipoFolio.Promocional => "Promocional",
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };
}
