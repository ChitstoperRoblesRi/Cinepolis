using Cinepolis.Domain;

namespace Cinepolis.Application;

/// <summary>Características de la sala que aloja la función.</summary>
public sealed record SalaLectura(int SalaId, int ComplejoId, string NombreSala, string TipoSala, int CapacidadAsientos);
/// <summary>Coordenadas reales, accesibilidad y disponibilidad de la butaca.</summary>
public sealed record AsientoLectura(int AsientoId, string Fila, int Columna, string? Numero, string TipoAsiento, string Estado, string AreaCategoryCode, bool EsAccesible);
/// <summary>Precio y cargo aplicables al área de la sala y tipo de boleto.</summary>
public sealed record TarifaFuncion(int TipoBoletoId, string Nombre, string AreaCategoryCode, decimal Precio, decimal Cargo);
/// <summary>Distribución de asientos y tarifas de una función.</summary>
public sealed record MapaSala(int FuncionId, SalaLectura Sala, IReadOnlyList<AsientoLectura> Asientos, IReadOnlyList<TarifaFuncion> Tarifas);
/// <summary>Consulta el mapa y las tarifas vigentes de una función.</summary>
public interface IFuncionAsientosRepository
{
    /// <summary>Recupera sala, coordenadas y disponibilidad para la función.</summary>
    Task<MapaSala?> LeerPorFuncionAsync(int funcionId, CancellationToken token);
}

/// <summary>Difunde cambios confirmados al grupo de la función.</summary>
public interface INotificadorSala
{
    /// <summary>Comunica el cambio confirmado a los clientes suscritos.</summary>
    Task PublicarAsync(CambioEstadoAsiento cambio, CancellationToken cancellationToken);
}

/// <summary>Accede al estado de las butacas de una función.</summary>
public sealed class ConsultarMapaSala(IFuncionAsientosRepository repositorio)
{
    /// <summary>Recupera la geometría, disponibilidad y tarifas de la sala.</summary>
    public Task<MapaSala?> EjecutarAsync(int funcionId, CancellationToken token) => repositorio.LeerPorFuncionAsync(funcionId, token);
}
