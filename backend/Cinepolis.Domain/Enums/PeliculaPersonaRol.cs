namespace Cinepolis.Domain;

/// <summary>Valores admitidos para pelicula persona rol.</summary>
public enum PeliculaPersonaRol
{
    Director,
    Actor
}

/// <summary>Convierte entre el valor tipado y su representación en PostgreSQL.</summary>
public static class ValoresPeliculaPersonaRol
{
    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public static PeliculaPersonaRol DesdeTexto(string valor) => valor switch
    {
        "director" => PeliculaPersonaRol.Director,
        "actor" => PeliculaPersonaRol.Actor,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };

    /// <summary>Devuelve la representación persistida del valor de dominio.</summary>
    public static string TextoSql(this PeliculaPersonaRol valor) => valor switch
    {
        PeliculaPersonaRol.Director => "director",
        PeliculaPersonaRol.Actor => "actor",
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };
}
