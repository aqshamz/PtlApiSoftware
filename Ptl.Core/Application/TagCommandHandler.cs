using Ptl.Agent.Application;
using Ptl.Agent.Domain;
using Ptl.Core.Interfaces;

namespace Ptl.Core.Application;

public class TagCommandHandler
{
    private readonly TransactionRunner _runner;

    public TagCommandHandler(TransactionRunner runner)
    {
        _runner = runner;
    }

    public void Handle(int tag, short cmd)
    {
        Console.WriteLine($"[HANDLER] tag={tag}, cmd={cmd}");
        if (!_runner.TryGetTagState(tag, out var state, out var tx))
            return;

        switch (cmd)
        {
            case PtlCommand.Decrease:
                _runner.DecreaseTag(state);
                Console.WriteLine($"DECREASE tag={tag} qty={state.Quantity}");
                break;

            case PtlCommand.Confirm:
                Console.WriteLine($"CONFIRM tag={tag}");
                _runner.CompleteTag(state, tx);
                break;
        }
    }
}
