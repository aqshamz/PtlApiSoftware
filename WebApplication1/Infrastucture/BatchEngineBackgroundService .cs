public class BatchEngineBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public BatchEngineBackgroundService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var engine = scope.ServiceProvider.GetRequiredService<BatchEngineService>();

            await engine.ProcessAsync();

            await Task.Delay(3000, stoppingToken);
        }
    }
}