using Ptl.Agent.Domain;

public class BatchEngineService
{
    private readonly ConnectedGatewayRegistry _registry;
    private readonly Phase2TagRegistry _tagRegistry;
    private readonly IBatchRepository _batchRepository;
    private readonly IPtlDisplay _display;
    private readonly IRecoveryState _recovery;

    public BatchEngineService(
        ConnectedGatewayRegistry registry,
        Phase2TagRegistry tagRegistry,
        IBatchRepository batchRepository,
        IPtlDisplay display,
        IRecoveryState recovery)
    {
        _registry = registry;
        _tagRegistry = tagRegistry;
        _batchRepository = batchRepository;
        _display = display;
        _recovery = recovery;
    }

    public async Task ProcessAsync()
    {
        var gateways = _registry.GetConnected().ToList(); //ambil gateaway avaiable
        if (!gateways.Any()) return;

        var tasks = new List<Task>();

        foreach (var gw in gateways)
        {
            if (string.IsNullOrEmpty(gw.TabelAwal))//kalo config bolong
            {
                PtlLog.Eng($"Gateway {gw.GatewayId} missing table config");
                continue;
            }

            if (_recovery.IsRecovering(gw.GatewayId)) //jika recovery sedang jalan di gateaway tersebut, tx baru gak diambil
            {
                PtlLog.Eng($"Gateway {gw.GatewayId} is recovering");
                continue;
            }

            tasks.Add(ProcessGateway(gw));
        }

        await Task.WhenAll(tasks); //tx paralel jalan per gateaway
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
            PtlLog.Eng($"Gateway {gw.GatewayId} failed to process: {ex.Message}");
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
            .GetNextTD3Header(batchNo, gw.IpAddress); //get header

        if (headerTag is not int tag)
            return;

        PtlLog.Eng($"Sending data header on Gateway {gw.GatewayId}");

        await _display.ClearHeader(gw.GatewayId, tag);

        await _display.ShowHeader(gw.GatewayId, tag, $"BATCH - {batchNo}");

        await _batchRepository.MarkTD3HeaderChecked(batchNo, gw.IpAddress); //update td3 send

        var td4Tags = await _batchRepository.GetPendingTD4Tags(batchNo, gw.IpAddress); //td4

        foreach (var td4Tag in td4Tags)
        {
            PtlLog.Eng($"Sending data test on tags Gateway {gw.GatewayId}");

            await _display.DisplayQty(gw.GatewayId, td4Tag, td4Tag);
            
            await _batchRepository.MarkTD4Checked(batchNo, td4Tag, "1", gw.IpAddress); //update td4 send
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
        var header = await _batchRepository.GetTd0Header(batchNo, plu, gw.IpAddress); //header batch TD0

        if (header == null)
            return;

        PtlLog.Eng($"Sending data header on Gateway {gw.GatewayId}");

        await _display.ClearHeader(gw.GatewayId, header.LokasiPtl);

        await _display.ShowHeader(gw.GatewayId, header.LokasiPtl, header.Descp);

        await _batchRepository.MarkTd0Checked(batchNo, plu, gw.IpAddress); //TD0 FLAGCEK1

        // D0 → D1
        var rows = await _batchRepository
        .GetD0Items(batchNo, plu, gw.IpAddress); //D0 Data

        foreach (var row in rows)
        {
            PtlLog.Eng($"Sending data on picking tags Gateway {gw.GatewayId}");

            await _display.DisplayQty(gw.GatewayId, row.LokasiPtl, row.OnPicking);

            var state = new TagState(
                gw.GatewayId,
                row.LokasiPtl,
                row.OnPicking,
                $"{batchNo}-{plu}"
            );

            _tagRegistry.Set(state);

            //flag_cek = '1' kuduny
            await _batchRepository.MarkD0AsD1(batchNo, plu, row.LokasiPtl, gw.IpAddress); //D0 flagcek 1
        }

        await _batchRepository
        .MarkSkuSending(batchNo, plu, 1, gw.TabelAwal); //str batch flag sending 1
    }

}