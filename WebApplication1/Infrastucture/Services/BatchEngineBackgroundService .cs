public class BatchEngineBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RecoveryService _recovery;
    private readonly DatabaseHealthService _dbHealth;

    public BatchEngineBackgroundService(IServiceScopeFactory scopeFactory, RecoveryService recovery, DatabaseHealthService dbHealth)
    {
        _scopeFactory = scopeFactory;
        _recovery = recovery;
        _dbHealth = dbHealth;
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
            if (!await _dbHealth.CheckAsync())
            {
                Console.WriteLine("[ENGINE] DB disconnected, pausing engine");
                await Task.Delay(3000, stoppingToken);
                continue;
            }

            using var scope = _scopeFactory.CreateScope();
            var engine = scope.ServiceProvider.GetRequiredService<BatchEngineService>();

            await engine.ProcessAsync();

            await Task.Delay(3000, stoppingToken);
        }
    }
}