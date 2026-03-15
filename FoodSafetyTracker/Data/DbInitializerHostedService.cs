namespace FoodSafetyTracker.Data;

public class DbInitializerHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public DbInitializerHostedService(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            await Task.Delay(800, cancellationToken);
            try { await DbInitializer.InitializeAsync(_serviceProvider); } catch { }
        }, cancellationToken);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
