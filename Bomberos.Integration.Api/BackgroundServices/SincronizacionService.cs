using Bomberos.Integration.Api.Common;
using Bomberos.Integration.Api.Services;
using Microsoft.Extensions.Options;

namespace Bomberos.Integration.Api.BackgroundServices;

/// <summary>Cada pocos minutos reintenta las operaciones pendientes y refresca la copia local de recursos.</summary>
public class SincronizacionService(
    IServiceScopeFactory scopes,
    IOptions<SyncOptions> options,
    ILogger<SincronizacionService> logger) : BackgroundService
{
    private readonly TimeSpan _intervalo = TimeSpan.FromMinutes(Math.Max(options.Value.IntervaloMinutos, 0.05));

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(_intervalo);
        try
        {
            do
            {
                try
                {
                    using var scope = scopes.CreateScope();
                    var sync = scope.ServiceProvider.GetRequiredService<ISyncService>();

                    var login = await sync.LoginServicioAsync(forzar: true);     // cuenta svc.integracion
                    if (!login.CoreDisponible)
                    {
                        await sync.RegistrarCoreCaidoAsync();                    // CORE sigue caído
                        continue;
                    }
                    if (login.Token is null) continue;

                    // Primero las pendientes, para que RESERVADO se reemplace por el estado real.
                    await sync.AplicarPendientesAsync(login.Token);
                    await sync.RefrescarCacheAsync(login.Token);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogError(ex, "Error en la sincronización");
                }
            }
            while (await timer.WaitForNextTickAsync(ct));
        }
        catch (OperationCanceledException)
        {
            // Apagado normal del host.
        }
    }
}
