namespace FoodSafetyTracker.Data;

public class DbInitializerHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public DbInitializerHostedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await DbInitializer.InitializeAsync(_serviceProvider);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
