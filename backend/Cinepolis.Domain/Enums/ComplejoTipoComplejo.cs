namespace Cinepolis.Domain;

/// <summary>Valores admitidos para complejo tipo complejo.</summary>
public enum ComplejoTipoComplejo
{
    Tradicional,
    VIP
}

/// <summary>Convierte entre el valor tipado y su representación en PostgreSQL.</summary>
public static class ValoresComplejoTipoComplejo
{
    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public static ComplejoTipoComplejo DesdeTexto(string valor) => valor switch
    {
        "Tradicional" => ComplejoTipoComplejo.Tradicional,
        "VIP" => ComplejoTipoComplejo.VIP,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };

    /// <summary>Devuelve la representación persistida del valor de dominio.</summary>
    public static string TextoSql(this ComplejoTipoComplejo valor) => valor switch
    {
        ComplejoTipoComplejo.Tradicional => "Tradicional",
        ComplejoTipoComplejo.VIP => "VIP",
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };
}
