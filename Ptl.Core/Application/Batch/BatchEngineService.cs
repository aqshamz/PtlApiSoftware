using Ptl.Agent.Domain;
using Ptl.Contracts.Dtos.Hardware;

public class BatchEngineService
{
    private readonly ConnectedGatewayRegistry _registry;
    private readonly Phase2TagRegistry _tagRegistry; 
    private readonly IBatchRepository _batchRepository;
    private readonly IPtlDisplay _display;

    public BatchEngineService(
        ConnectedGatewayRegistry registry,
        Phase2TagRegistry tagRegistry,
        IBatchRepository batchRepository,
        IPtlDisplay display)
    {
        _registry = registry;
        _tagRegistry = tagRegistry;
        _batchRepository = batchRepository;
        _display = display;
    }

    public async Task ProcessAsync()
    {
        var gateways = _registry.GetConnected().ToList(); //ambil gateaway avaiable
        if (!gateways.Any()) return;

        await Task.WhenAll( 
            gateways.Select(ProcessGateway) //jalankan paralel transaksi tiap gateaway
        );
    }

    private async Task ProcessGateway(GatewayRuntimeInfo gw)
    {
        try
        {
            await HandlePhase1(gw);
            await HandlePhase2(gw);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ENGINE] Gateway {gw.GatewayId} failed: {ex.Message}");
        }
    }

    // =============================
    // PHASE 1 (flag_batch = 4)
    // =============================
    private async Task HandlePhase1(GatewayRuntimeInfo gw)
    {
        var batchNoNullable = await _batchRepository
        .GetNextBatchAsync(gw.TabelAwal, 4); //get flagbatch 4

        if (batchNoNullable is not int batchNo)
            return;

        var headerTag = await _batchRepository
            .GetNextTD3Header(batchNo); //get header

        if (headerTag is not int tag)
            return;

        await _display.ClearHeader(gw.GatewayId, tag);

        await _display.ShowHeader(gw.GatewayId, tag, $"BATCH - {batchNo}");

        await _batchRepository.MarkTD3HeaderChecked(batchNo); //update td3 send

        var td4Tags = await _batchRepository
        .GetPendingTD4Tags    (batchNo); //td4

        foreach (var td4Tag in td4Tags)
        {
            await _display.DisplayQty(gw.GatewayId, td4Tag, td4Tag);
            
            await _batchRepository.MarkTD4Checked(batchNo, td4Tag, "1"); //update td4 send
        }
    }

    // =============================
    // PHASE 2 (flag_batch = 1)
    // =============================
    private async Task HandlePhase2(GatewayRuntimeInfo gw)
    {
        var sku = await _batchRepository
            .GetNextSkuForPhase2(gw.TabelAwal);

        if (sku == null)
            return;

        if (sku.FlagSending == 0)
        {
            await LoadSku(gw, sku.BatchNo, sku.Plu);
            return;
        }
    }

    private async Task LoadSku(
        GatewayRuntimeInfo gw,
        int batchNo,
        int plu)
    {
        // TD0 Header
        var header = await _batchRepository
        .GetTd0Header(batchNo, plu); //header batch TD0

        if (header == null)
            return;

        await _display.ClearHeader(gw.GatewayId, header.LokasiPtl);

        await _display.ShowHeader(gw.GatewayId, header.LokasiPtl, header.Descp);

        await _batchRepository.MarkTd0Checked(batchNo, plu); //TD0 FLAGCEK1

        // D0 → D1
        var rows = await _batchRepository
        .GetD0Items(batchNo, plu); //D0 Data

        foreach (var row in rows)
        {
            await _display.DisplayQty(gw.GatewayId, row.LokasiPtl, row.OnPicking);

            var state = new TagState(
                gw.GatewayId,
                row.LokasiPtl,
                row.OnPicking,
                $"{batchNo}-{plu}"
            );

            _tagRegistry.Set(state);

            //flag_cek = '1' kuduny
            await _batchRepository
            .MarkD0AsD1(batchNo, plu, row.LokasiPtl); //D0 flagcek 1
        }

        await _batchRepository
        .MarkSkuSending(batchNo, plu, 1, gw.TabelAwal); //str batch flag sending 1
    }

}