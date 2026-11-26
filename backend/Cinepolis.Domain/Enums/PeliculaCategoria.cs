namespace Cinepolis.Domain;

public enum PeliculaCategoria
{
    Estreno,
    Preventa,
    Cartelera,
    EnCartelera,
    GarantiaCinepolis,
    Evento,
    ContenidoAlternativo,
    Festival
}

public static class ValoresPeliculaCategoria
{
    public static PeliculaCategoria DesdeTexto(string valor) => valor switch
    {
        "Estreno" => PeliculaCategoria.Estreno,
        "Preventa" => PeliculaCategoria.Preventa,
        "Cartelera" => PeliculaCategoria.Cartelera,
        "En Cartelera" => PeliculaCategoria.EnCartelera,
        "Garantia Cinepolis" => PeliculaCategoria.GarantiaCinepolis,
        "Evento" => PeliculaCategoria.Evento,
        "Contenido Alternativo" => PeliculaCategoria.ContenidoAlternativo,
        "Festival" => PeliculaCategoria.Festival,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };

    public static string TextoSql(this PeliculaCategoria valor) => valor switch
    {
        PeliculaCategoria.Estreno => "Estreno",
        PeliculaCategoria.Preventa => "Preventa",
        PeliculaCategoria.Cartelera => "Cartelera",
        PeliculaCategoria.EnCartelera => "En Cartelera",
        PeliculaCategoria.GarantiaCinepolis => "Garantia Cinepolis",
        PeliculaCategoria.Evento => "Evento",
        PeliculaCategoria.ContenidoAlternativo => "Contenido Alternativo",
        PeliculaCategoria.Festival => "Festival",
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };
}
