namespace Cinepolis.Domain;

/// <summary>Valores admitidos para folio estado.</summary>
public enum FolioEstado
{
    Activo,
    Usado,
    Vencido
}

/// <summary>Convierte entre el valor tipado y su representación en PostgreSQL.</summary>
public static class ValoresFolioEstado
{
    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public static FolioEstado DesdeTexto(string valor) => valor switch
    {
        "activo" => FolioEstado.Activo,
        "usado" => FolioEstado.Usado,
        "vencido" => FolioEstado.Vencido,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };

    /// <summary>Devuelve la representación persistida del valor de dominio.</summary>
    public static string TextoSql(this FolioEstado valor) => valor switch
    {
        FolioEstado.Activo => "activo",
        FolioEstado.Usado => "usado",
        FolioEstado.Vencido => "vencido",
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };
}
