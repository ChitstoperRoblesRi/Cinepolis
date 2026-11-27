using Cinepolis.Application;
using Cinepolis.Domain;
using Microsoft.AspNetCore.SignalR;

namespace Cinepolis.API;

/// <summary>Coordina suscripciones y reservas en tiempo real por función.</summary>
public sealed class SalaHub(IReservaAsientosRepository asientos, INotificadorSala notificador, CredencialReserva reservas) : Hub
{
    /// <summary>Emite una credencial vinculada a la conexión SignalR.</summary>
    public string ObtenerTokenReserva() => reservas.Emitir(Context.ConnectionId);
    internal static string Grupo(int funcionId) => $"funcion-{funcionId}";
    /// <summary>Suscribe la conexión a los cambios de la función.</summary>
    public Task UnirseAFuncion(int funcionId) => Groups.AddToGroupAsync(Context.ConnectionId, Grupo(funcionId));
    /// <summary>Suscribe la conexión a los cambios de la función.</summary>
    public Task SeguirFuncion(int funcionId) => UnirseAFuncion(funcionId);
    /// <summary>Cancela la suscripción a los cambios de la función.</summary>
    public Task DejarFuncion(int funcionId) => Groups.RemoveFromGroupAsync(Context.ConnectionId, Grupo(funcionId));

    /// <summary>Reserva una butaca disponible para la conexión solicitante.</summary>
    public async Task<bool> SeleccionarAsiento(int funcionId, int asientoId)
    {
        if (!await asientos.SeleccionarAsync(funcionId, asientoId, Context.ConnectionId, Context.ConnectionAborted)) return false;
        await notificador.PublicarAsync(new(funcionId, asientoId, AsientoEstado.Reservado.TextoSql()), CancellationToken.None);
        return true;
    }

    /// <summary>Libera una butaca reservada por la conexión solicitante.</summary>
    public async Task<bool> LiberarAsiento(int funcionId, int asientoId)
    {
        if (!await asientos.LiberarAsync(funcionId, asientoId, Context.ConnectionId, Context.ConnectionAborted)) return false;
        await notificador.PublicarAsync(new(funcionId, asientoId, AsientoEstado.Disponible.TextoSql()), CancellationToken.None);
        return true;
    }
}

/// <summary>Difunde cambios confirmados al grupo de la función.</summary>
public sealed class NotificadorSala(IHubContext<SalaHub> hub) : INotificadorSala
{
    /// <summary>Comunica el cambio confirmado a los clientes suscritos.</summary>
    public Task PublicarAsync(CambioEstadoAsiento cambio, CancellationToken cancellationToken) =>
        hub.Clients.Group(SalaHub.Grupo(cambio.FuncionId)).SendAsync("AsientoActualizado", cambio, cancellationToken);
}
