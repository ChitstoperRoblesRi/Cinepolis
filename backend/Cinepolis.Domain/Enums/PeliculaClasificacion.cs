namespace Cinepolis.Domain;

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

public static class ValoresPeliculaClasificacion
{
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
