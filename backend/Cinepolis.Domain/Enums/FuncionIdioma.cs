namespace Cinepolis.Domain;

public enum FuncionIdioma
{
    Español,
    Subtitulada,
    Doblada
}

public static class ValoresFuncionIdioma
{
    public static FuncionIdioma DesdeTexto(string valor) => valor switch
    {
        "Español" => FuncionIdioma.Español,
        "Subtitulada" => FuncionIdioma.Subtitulada,
        "Doblada" => FuncionIdioma.Doblada,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };

    public static string TextoSql(this FuncionIdioma valor) => valor switch
    {
        FuncionIdioma.Español => "Español",
        FuncionIdioma.Subtitulada => "Subtitulada",
        FuncionIdioma.Doblada => "Doblada",
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };
}
