using Cinepolis.Application;
using Cinepolis.Domain;

namespace Cinepolis.API;

/// <summary>Propaga los cambios confirmados, registrando fallos individuales de entrega.</summary>
public sealed class NotificadorVenta(INotificadorSala sala, ILogger<NotificadorVenta> log) : INotificadorVenta
{
    /// <inheritdoc/>
    public async Task PublicarAsync(Ticket ticket)
    {
        foreach (var boleto in ticket.Boletos)
        {
            try { await sala.PublicarAsync(new(boleto.FuncionId, boleto.AsientoId, AsientoEstado.Vendido.TextoSql()), CancellationToken.None); }
            catch (Exception ex) { log.LogError(ex, "Fallo al notificar la venta del asiento {AsientoId}", boleto.AsientoId); }
        }
    }
}
