namespace Cinepolis.Domain;

/// <summary>Valores admitidos para folio tipo uso.</summary>
public enum FolioTipoUso
{
    Boletos,
    Alimentos
}

/// <summary>Convierte entre el valor tipado y su representación en PostgreSQL.</summary>
public static class ValoresFolioTipoUso
{
    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public static FolioTipoUso DesdeTexto(string valor) => valor switch
    {
        "boletos" => FolioTipoUso.Boletos,
        "alimentos" => FolioTipoUso.Alimentos,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };

    /// <summary>Devuelve la representación persistida del valor de dominio.</summary>
    public static string TextoSql(this FolioTipoUso valor) => valor switch
    {
        FolioTipoUso.Boletos => "boletos",
        FolioTipoUso.Alimentos => "alimentos",
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };
}
