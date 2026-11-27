namespace Cinepolis.Domain;

/// <summary>Valores admitidos para modificador regla tipo modificador.</summary>
public enum ModificadorReglaTipoModificador
{
    Flavor,
    Size,
    Extra,
    Ingredient,
    Popcorn,
    Compound
}

/// <summary>Convierte entre el valor tipado y su representación en PostgreSQL.</summary>
public static class ValoresModificadorReglaTipoModificador
{
    /// <summary>Interpreta el valor textual almacenado por PostgreSQL.</summary>
    public static ModificadorReglaTipoModificador DesdeTexto(string valor) => valor switch
    {
        "flavor" => ModificadorReglaTipoModificador.Flavor,
        "size" => ModificadorReglaTipoModificador.Size,
        "extra" => ModificadorReglaTipoModificador.Extra,
        "ingredient" => ModificadorReglaTipoModificador.Ingredient,
        "popcorn" => ModificadorReglaTipoModificador.Popcorn,
        "compound" => ModificadorReglaTipoModificador.Compound,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };

    /// <summary>Devuelve la representación persistida del valor de dominio.</summary>
    public static string TextoSql(this ModificadorReglaTipoModificador valor) => valor switch
    {
        ModificadorReglaTipoModificador.Flavor => "flavor",
        ModificadorReglaTipoModificador.Size => "size",
        ModificadorReglaTipoModificador.Extra => "extra",
        ModificadorReglaTipoModificador.Ingredient => "ingredient",
        ModificadorReglaTipoModificador.Popcorn => "popcorn",
        ModificadorReglaTipoModificador.Compound => "compound",
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null)
    };
}
