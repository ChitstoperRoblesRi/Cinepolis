namespace Cinepolis.Application;

public sealed record SalaLectura(int SalaId, int ComplejoId, string NombreSala, string TipoSala, int CapacidadAsientos);
public sealed record AsientoLectura(int AsientoId, string Fila, int Columna, string? Numero, string TipoAsiento, string Estado, string AreaCategoryCode, bool EsAccesible);
public sealed record TarifaFuncion(int TipoBoletoId, string Nombre, string AreaCategoryCode, decimal Precio, decimal Cargo);
public sealed record MapaSala(int FuncionId, SalaLectura Sala, IReadOnlyList<AsientoLectura> Asientos, IReadOnlyList<TarifaFuncion> Tarifas);
public sealed record CambioEstadoAsiento(int FuncionId, int AsientoId, string Estado);

public interface IAsientoRepository
{
    Task<MapaSala?> LeerPorFuncionAsync(int funcionId, CancellationToken token);
    Task<bool> SeleccionarAsync(int funcionId, int asientoId, string conexion, CancellationToken token);
    Task<bool> LiberarAsync(int funcionId, int asientoId, string conexion, CancellationToken token);
    Task<IReadOnlyList<CambioEstadoAsiento>> LiberarVencidosAsync(CancellationToken token);
}

public interface INotificadorSala
{
    Task PublicarAsync(CambioEstadoAsiento cambio, CancellationToken cancellationToken);
}

public sealed class ConsultarMapaSala(IAsientoRepository repositorio)
{
    public Task<MapaSala?> EjecutarAsync(int funcionId, CancellationToken token) => repositorio.LeerPorFuncionAsync(funcionId, token);
}
