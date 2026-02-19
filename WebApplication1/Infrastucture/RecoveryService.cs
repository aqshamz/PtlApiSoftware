using Ptl.Agent.Application;
using Ptl.Core.Interfaces;

public class RecoveryService : IHostedService
{
    private readonly TransactionRunner _runner;
    private readonly ITransactionSource _source;

    public RecoveryService(TransactionRunner runner, ITransactionSource source)
    {
        _runner = runner;
        _source = source;
    }

    public Task StartAsync(CancellationToken ct)
    {
        Console.WriteLine("[RECOVERY] Loading active transactions");

        foreach (var dto in _source.GetActiveTransactions())
        {
            var tx = _runner.BuildTransaction(dto);
            _runner.RestoreTransaction(tx);
        }

        _runner.RecoveryCompleted = true;

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
