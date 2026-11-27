namespace Cinepolis.Domain;

/// <summary>Valores admitidos para pelicula categoria.</summary>
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

/// <summary>Convierte entre el valor tipado y su representación en PostgreSQL.</summary>
public static class ValoresPeliculaCategoria
{
    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
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

    /// <summary>Devuelve la representación persistida del valor de dominio.</summary>
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
