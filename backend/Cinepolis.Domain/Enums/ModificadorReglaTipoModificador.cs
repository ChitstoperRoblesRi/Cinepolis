namespace Cinepolis.Domain;

public enum ModificadorReglaTipoModificador
{
    Flavor,
    Size,
    Extra,
    Ingredient,
    Popcorn,
    Compound
}

public static class ValoresModificadorReglaTipoModificador
{
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
