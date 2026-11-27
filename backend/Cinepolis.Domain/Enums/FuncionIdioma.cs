namespace Cinepolis.Domain;

/// <summary>Valores admitidos para funcion idioma.</summary>
public enum FuncionIdioma
{
    Español,
    Subtitulada,
    Doblada
}

/// <summary>Convierte entre el valor tipado y su representación en PostgreSQL.</summary>
public static class ValoresFuncionIdioma
{
    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public static FuncionIdioma DesdeTexto(string valor) => valor switch
    {
        "Español" => FuncionIdioma.Español,
        "Subtitulada" => FuncionIdioma.Subtitulada,
        "Doblada" => FuncionIdioma.Doblada,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };

    /// <summary>Devuelve la representación persistida del valor de dominio.</summary>
    public static string TextoSql(this FuncionIdioma valor) => valor switch
    {
        FuncionIdioma.Español => "Español",
        FuncionIdioma.Subtitulada => "Subtitulada",
        FuncionIdioma.Doblada => "Doblada",
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };
}
