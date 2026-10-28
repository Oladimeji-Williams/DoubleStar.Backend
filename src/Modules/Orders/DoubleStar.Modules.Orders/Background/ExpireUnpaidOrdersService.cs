// Wakes every minute and asks the application layer to expire stale unpaid orders (releasing their stock).
// All logic lives in ExpireUnpaidOrdersCommandHandler — this class is only the clock.
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DoubleStar.Modules.Orders.Application.Commands.ExpireUnpaidOrders;

namespace DoubleStar.Modules.Orders.Background;

internal sealed class ExpireUnpaidOrdersService(
    IServiceScopeFactory scopeFactory,
    IHostEnvironment environment,
    ILogger<ExpireUnpaidOrdersService> logger)
    : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Integration tests run the real host; a sweeper ticking in the background would make them flaky.
        if (environment.IsEnvironment("Testing"))
        {
            return;
        }

        using var timer = new PeriodicTimer(Interval);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using var scope = scopeFactory.CreateScope();
                    var sender = scope.ServiceProvider.GetRequiredService<ISender>();

                    var result = await sender.Send(new ExpireUnpaidOrdersCommand(), stoppingToken);

                    if (result.IsFailure)
                    {
                        logger.LogWarning(
                            "Order expiry sweep failed: {Errors}", string.Join("; ", result.Errors.Select(e => e.Code)));
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogError(ex, "Order expiry sweep crashed; it will retry on the next tick.");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // normal shutdown
        }
    }
}
