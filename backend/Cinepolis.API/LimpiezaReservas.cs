using Cinepolis.Domain;
using Cinepolis.Application;

namespace Cinepolis.API;

/// <summary>Procesa las reservas vencidas y publica las butacas liberadas.</summary>
public sealed class LimpiezaReservas(IServiceScopeFactory scopes, INotificadorSala notificador,
    ILogger<LimpiezaReservas> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var reloj = new PeriodicTimer(TimeSpan.FromSeconds(30));
        while (await reloj.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = scopes.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<IReservasVencidasRepository>();
                while (!stoppingToken.IsCancellationRequested)
                {
                    var cambios = await repo.LiberarVencidosAsync(stoppingToken);
                    if (cambios.Count == 0) break;
                    foreach (var cambio in cambios)
                    {
                        try
                        {
                            await notificador.PublicarAsync(cambio, stoppingToken);
                        }
                        catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                        {
                            logger.LogError(ex, "Error al notificar la liberación del asiento {AsientoId}", cambio.AsientoId);
                        }
                    }
                }
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError(ex, "Error al liberar reservas vencidas");
            }
        }
    }
}
