namespace Cinepolis.Domain;

public enum ComplejoTipoComplejo
{
    Tradicional,
    VIP
}

public static class ValoresComplejoTipoComplejo
{
    public static ComplejoTipoComplejo DesdeTexto(string valor) => valor switch
    {
        "Tradicional" => ComplejoTipoComplejo.Tradicional,
        "VIP" => ComplejoTipoComplejo.VIP,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };

    public static string TextoSql(this ComplejoTipoComplejo valor) => valor switch
    {
        ComplejoTipoComplejo.Tradicional => "Tradicional",
        ComplejoTipoComplejo.VIP => "VIP",
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };
}
