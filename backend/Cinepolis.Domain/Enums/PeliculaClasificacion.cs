namespace Cinepolis.Domain;

/// <summary>Valores admitidos para pelicula clasificacion.</summary>
public enum PeliculaClasificacion
{
    A,
    AA,
    B,
    B15,
    C,
    D,
    TBC,
    SC
}

/// <summary>Convierte entre el valor tipado y su representación en PostgreSQL.</summary>
public static class ValoresPeliculaClasificacion
{
    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public static PeliculaClasificacion DesdeTexto(string valor) => valor switch
    {
        "A" => PeliculaClasificacion.A,
        "AA" => PeliculaClasificacion.AA,
        "B" => PeliculaClasificacion.B,
        "B15" => PeliculaClasificacion.B15,
        "C" => PeliculaClasificacion.C,
        "D" => PeliculaClasificacion.D,
        "TBC" => PeliculaClasificacion.TBC,
        "S/C" => PeliculaClasificacion.SC,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };

    /// <summary>Devuelve la representación persistida del valor de dominio.</summary>
    public static string TextoSql(this PeliculaClasificacion valor) => valor switch
    {
        PeliculaClasificacion.A => "A",
        PeliculaClasificacion.AA => "AA",
        PeliculaClasificacion.B => "B",
        PeliculaClasificacion.B15 => "B15",
        PeliculaClasificacion.C => "C",
        PeliculaClasificacion.D => "D",
        PeliculaClasificacion.TBC => "TBC",
        PeliculaClasificacion.SC => "S/C",
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };
}
