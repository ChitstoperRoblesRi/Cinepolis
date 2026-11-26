namespace Cinepolis.Domain;

public enum PeliculaPersonaRol
{
    Director,
    Actor
}

public static class ValoresPeliculaPersonaRol
{
    public static PeliculaPersonaRol DesdeTexto(string valor) => valor switch
    {
        "director" => PeliculaPersonaRol.Director,
        "actor" => PeliculaPersonaRol.Actor,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };

    public static string TextoSql(this PeliculaPersonaRol valor) => valor switch
    {
        PeliculaPersonaRol.Director => "director",
        PeliculaPersonaRol.Actor => "actor",
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };
}
