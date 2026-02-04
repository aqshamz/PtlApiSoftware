using Ptl.Agent.Application;

public class PickEventRetryWorker : BackgroundService
{
    private readonly IPickEventStore _store;
    private readonly TransactionRunner _runner;

    public PickEventRetryWorker(
        IPickEventStore store,
        TransactionRunner runner)
    {
        _store = store;
        _runner = runner;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("[RETRY] PickEventRetryWorker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var evt in _store.LoadUnprocessed())
            {
                if (_runner.TryApply(evt))
                {
                    _store.MarkProcessed(evt.EventId);
                    Console.WriteLine($"[RETRY] Applied event {evt.EventId}");
                }
            }

            await Task.Delay(2000, stoppingToken);
        }
    }
}
