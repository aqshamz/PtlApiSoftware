using Microsoft.Extensions.Hosting;
using Ptl.Agent.Application;

public class TransactionLoaderService : BackgroundService
{
    private readonly TransactionRunner _runner;

    public TransactionLoaderService(TransactionRunner runner)
    {
        _runner = runner;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("[LOADER] Transaction loader started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var tx = _runner.GetNextTransaction();
                if (tx != null)
                {
                    Console.WriteLine($"[LOADER] Starting TX {tx.TransactionId}");
                    _runner.StartTransaction(tx);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LOADER][ERROR] {ex.Message}");
            }

            await Task.Delay(1000, stoppingToken); // 1s poll
        }
    }
}
