using Cinepolis.Application;
using Cinepolis.Domain;
using Microsoft.AspNetCore.SignalR;

namespace Cinepolis.API;

public sealed class SalaHub(IAsientoRepository asientos, INotificadorSala notificador, CredencialReserva reservas) : Hub
{
    public string ObtenerTokenReserva() => reservas.Emitir(Context.ConnectionId);
    internal static string Grupo(int funcionId) => $"funcion-{funcionId}";
    public Task UnirseAFuncion(int funcionId) => Groups.AddToGroupAsync(Context.ConnectionId, Grupo(funcionId));
    public Task SeguirFuncion(int funcionId) => UnirseAFuncion(funcionId);
    public Task DejarFuncion(int funcionId) => Groups.RemoveFromGroupAsync(Context.ConnectionId, Grupo(funcionId));

    public async Task<bool> SeleccionarAsiento(int funcionId, int asientoId)
    {
        if (!await asientos.SeleccionarAsync(funcionId, asientoId, Context.ConnectionId, Context.ConnectionAborted)) return false;
        await notificador.PublicarAsync(new(funcionId, asientoId, AsientoEstado.Reservado.TextoSql()), CancellationToken.None);
        return true;
    }

    public async Task<bool> LiberarAsiento(int funcionId, int asientoId)
    {
        if (!await asientos.LiberarAsync(funcionId, asientoId, Context.ConnectionId, Context.ConnectionAborted)) return false;
        await notificador.PublicarAsync(new(funcionId, asientoId, AsientoEstado.Disponible.TextoSql()), CancellationToken.None);
        return true;
    }
}

public sealed class NotificadorSala(IHubContext<SalaHub> hub) : INotificadorSala
{
    public Task PublicarAsync(CambioEstadoAsiento cambio, CancellationToken cancellationToken) =>
        hub.Clients.Group(SalaHub.Grupo(cambio.FuncionId)).SendAsync("AsientoActualizado", cambio, cancellationToken);
}
