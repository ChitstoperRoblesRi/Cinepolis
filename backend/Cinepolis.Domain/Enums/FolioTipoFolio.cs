namespace Cinepolis.Domain;

/// <summary>Valores admitidos para folio tipo folio.</summary>
public enum FolioTipoFolio
{
    Corporativo,
    Mastercard2x1,
    Cinepass,
    Promocional
}

/// <summary>Convierte entre el valor tipado y su representación en PostgreSQL.</summary>
public static class ValoresFolioTipoFolio
{
    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public static FolioTipoFolio DesdeTexto(string valor) => valor switch
    {
        "Corporativo" => FolioTipoFolio.Corporativo,
        "Mastercard2x1" => FolioTipoFolio.Mastercard2x1,
        "Cinepass" => FolioTipoFolio.Cinepass,
        "Promocional" => FolioTipoFolio.Promocional,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };

    /// <summary>Devuelve la representación persistida del valor de dominio.</summary>
    public static string TextoSql(this FolioTipoFolio valor) => valor switch
    {
        FolioTipoFolio.Corporativo => "Corporativo",
        FolioTipoFolio.Mastercard2x1 => "Mastercard2x1",
        FolioTipoFolio.Cinepass => "Cinepass",
        FolioTipoFolio.Promocional => "Promocional",
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };
}
