namespace Cinepolis.Domain;

/// <summary>Entidad de funcion del sistema de cine.</summary>
public sealed class Funcion
{
    public int FuncionId { get; init; }

    public DateOnly FechaFuncion { get; init; }

    public TimeOnly HoraInicio { get; init; }

    public TimeOnly HoraTermino { get; init; }

    public required string Idioma { get; init; }

    public bool EsPreventa { get; init; }

    public int FormatoId { get; init; }

    public int SalaId { get; init; }

    public int PeliculaId { get; init; }

    public bool Activa { get; init; }

    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public FuncionIdioma IdiomaTipado => ValoresFuncionIdioma.DesdeTexto(Idioma);
}
