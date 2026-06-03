using Microsoft.Extensions.Hosting;
using CanteenReservationSystem.Services.Interfaces;

namespace CanteenReservationSystem.Services;

public class OrderAutoCloseService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public OrderAutoCloseService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;

            // Run at 23:59 every day
            var nextRun = DateTime.Today.AddDays(1).AddMinutes(-1);

            var delay = nextRun - now;

            if (delay.TotalMilliseconds > 0)
                await Task.Delay(delay, stoppingToken);

            await ProcessUncompletedOrders();
        }
        
    }

    private async Task ProcessUncompletedOrders()
    {
        using var scope = _scopeFactory.CreateScope();
        var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

        await orderService.MarkAllPendingAsNotTakenAsync();
    }
}