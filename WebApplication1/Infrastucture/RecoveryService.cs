using Microsoft.Win32;
using Ptl.Agent.Domain;
using Ptl.Core.Application;
using Ptl.Core.Interfaces;

public class RecoveryService : IHostedService, IRecoveryState
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly Phase2TagRegistry _tagRegistry;
    private readonly IPtlDisplay _display;
    private readonly ConnectedGatewayRegistry _registry;
    public bool Completed { get; private set; } = false;
    private readonly HashSet<int> _recoveringGateways = new();

    public RecoveryService(
        IServiceScopeFactory scopeFactory,
        Phase2TagRegistry tagRegistry,
        IPtlDisplay display,
        ConnectedGatewayRegistry registry)
    {
        _scopeFactory = scopeFactory;
        _tagRegistry = tagRegistry;
        _display = display;
        _registry = registry;
    }

    public bool IsRecovering(int gatewayId)
    {
        lock (_recoveringGateways)
        {
            return _recoveringGateways.Contains(gatewayId);
        }
    }

    public async Task StartAsync(CancellationToken ct)
    {
        Console.WriteLine("[RECOVERY] Checking unfinished picks...");

        var gateways = _registry.GetConnected().ToList();

        foreach (var gw in gateways)
        {
            await RecoverGateway(gw);
        }

        Completed = true;

        Console.WriteLine("[RECOVERY] Completed");
    }

    public async Task RecoverGateway(GatewayRuntimeInfo gw)
    {
        lock (_recoveringGateways)
        {
            if (_recoveringGateways.Contains(gw.GatewayId))
                return;

            _recoveringGateways.Add(gw.GatewayId);
        }

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IBatchRepository>();


            // PHASE 1 CHECK
            if (await repo.Phase1RecoveryPending(gw.IpAddress))
            {
                Console.WriteLine($"[RECOVERY] Gateway {gw.GatewayId} recovering");
                var batchNoNullable = await repo.GetNextBatchAsync(gw.TabelAwal, 4);

                if (batchNoNullable is not int batchNo)
                    return;

                var tag = await repo.GetNextTD3Header(batchNo, gw.IpAddress);

                if (tag is null)
                    tag = await repo.GetNextTD3HeaderRecovery(batchNo, gw.IpAddress);

                if (tag is null)
                    return;

                int headerTag = tag.Value;

                await _display.ClearHeader(gw.GatewayId, headerTag);
                await _display.ShowHeader(gw.GatewayId, headerTag, $"BATCH - {batchNo}");

                await repo.MarkTD3HeaderChecked(batchNo, gw.IpAddress);

                var td4Tags = await repo.GetPendingAllTD4Tags(batchNo, gw.IpAddress);

                foreach (var td4Tag in td4Tags)
                {
                    await _display.DisplayQty(gw.GatewayId, td4Tag, td4Tag);
                    await repo.MarkTD4Checked(batchNo, td4Tag, "1", gw.IpAddress);
                }

                Console.WriteLine($"[RECOVERY] Phase1 restore for gateway {gw.GatewayId}");
                return;
            }

            // PHASE 2 CHECK
            var sku = await repo.GetNextSkuForPhase2(gw.TabelAwal);
            if (sku == null)
                return;

            if (sku.FlagSending == 1)
            {
                Console.WriteLine($"[RECOVERY] Phase2 restore {sku.BatchNo}-{sku.Plu} for gateway {gw.GatewayId}");
                await LoadSku(gw, sku.BatchNo, sku.Plu);
            }

             return;

        }
        finally {
            lock (_recoveringGateways)
            {
                _recoveringGateways.Remove(gw.GatewayId);
            }
            Console.WriteLine($"[RECOVERY] Gateway {gw.GatewayId} finished");
        }
           
    }
    
    private async Task LoadSku(
        GatewayRuntimeInfo gw,
        int batchNo,
        int plu)
    {

        using var scope = _scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IBatchRepository>();

        // TD0 Header
        var header = await repo.GetTd0Header(batchNo, plu, gw.IpAddress); //header batch TD0

        if (header == null)
            return;

        await _display.ClearHeader(gw.GatewayId, header.LokasiPtl);

        await _display.ShowHeader(gw.GatewayId, header.LokasiPtl, header.Descp);

        await repo.MarkTd0Checked(batchNo, plu, gw.IpAddress); //TD0 FLAGCEK1

        // D0 and D1
        var rows = await repo.GetD0D1Items(batchNo, plu, gw.IpAddress); //D0 D1 Data

        foreach (var row in rows)
        {
            await _display.DisplayQty(gw.GatewayId, row.LokasiPtl, row.OnPicking);

            var state = new TagState(
                gw.GatewayId,
                row.LokasiPtl,
                row.OnPicking,
                $"{batchNo}-{plu}"
            );

            if (!_tagRegistry.Exists(row.LokasiPtl))
            {
                _tagRegistry.Set(state);
            }

            //flag_cek = '1' kuduny
            await repo.MarkD0AsD1(batchNo, plu, row.LokasiPtl, gw.IpAddress); //D0 flagcek 1
        }
    }

    public Task StopAsync(CancellationToken ct)
        => Task.CompletedTask;
}