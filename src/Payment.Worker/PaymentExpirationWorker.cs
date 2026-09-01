using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Payment.Worker;

public sealed class PaymentExpirationWorker(
    PaymentMaintenanceClient maintenanceClient,
    ILogger<PaymentExpirationWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                var expiredCount = await maintenanceClient.ExpirePendingAsync(stoppingToken);
                if (expiredCount > 0)
                {
                    logger.LogInformation("{Count} payment(s) expired", expiredCount);
                }
            }
            catch (HttpRequestException exception)
            {
                logger.LogError(exception, "Could not run payment expiration");
            }
        }
    }
}
