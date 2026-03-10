public class BatchEngineBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RecoveryService _recovery;

    public BatchEngineBackgroundService(IServiceScopeFactory scopeFactory, RecoveryService recovery)
    {
        _scopeFactory = scopeFactory;
        _recovery = recovery;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!_recovery.Completed)
        {
            Console.WriteLine("[ENGINE] Waiting for recovery...");
            await Task.Delay(500, stoppingToken);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var engine = scope.ServiceProvider.GetRequiredService<BatchEngineService>();

            await engine.ProcessAsync();

            await Task.Delay(3000, stoppingToken);
        }
    }
}