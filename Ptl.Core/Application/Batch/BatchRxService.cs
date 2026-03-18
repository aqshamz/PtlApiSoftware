using Ptl.Agent.Domain;
using Ptl.Contracts.Dtos.Hardware;

public class BatchRxService
{
    private readonly ConnectedGatewayRegistry _registry;
    private readonly Phase2TagRegistry _tagRegistry;
    private readonly IBatchRepository _batchRepository;
    private readonly IPtlDisplay _display;

    public BatchRxService(
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

    public async Task HandleAsync(PtlRxEventDto evt)
    {
        var gw = _registry.GetConnected()
            .FirstOrDefault(g => g.GatewayId == evt.Gateway);

        if (gw == null)
            return;

        PtlLog.Rx($"Received Command {evt.Command} from Tag {evt.Tag}");

        // Phase 1 (batch 4)
        await HandlePhase1Async(gw, evt.Tag);

        // Phase 2 Decrease
        if (evt.Command == PtlCommand.Decrease)
        {
            if (!_tagRegistry.TryGet(evt.Tag, out var state))
                return;

            if (state.Quantity <= 0)
                return;

            state.Decrease();

            await _display.DisplayQty(
                state.Gateaway,
                state.Tag,
                state.Quantity
            );

            PtlLog.Rx($"Decrease qty into {state.Quantity} on tag {evt.Tag}");
            return;
        }

        // Phase 2 Confirm
        if (evt.Command == PtlCommand.Confirm)
        {
            await HandlePhase2ConfirmAsync(gw, evt.Tag);
        }
    }

    // =============================
    // PHASE 1
    // =============================
    private async Task HandlePhase1Async(
        GatewayRuntimeInfo gw,
        int tag)
    {
        var batchNoNullable = await _batchRepository
        .GetNextBatchAsync(gw.TabelAwal, 4); //get flagbatch 4

        if (batchNoNullable is not int batchNo)
            return;

        PtlLog.Rx($"Confirming Data Tag {tag}");

        await _batchRepository.MarkTD4Checked(batchNo, tag, "2", gw.IpAddress); //update td4 receive

        if (await _batchRepository.HasPendingTd4(batchNo, gw.IpAddress)) //check if td4 still active
            return;

        await _batchRepository.UpdateFlagBatchUrut(batchNo, 0, gw.TabelAwal); // Mark batch 0

        // Clear header
        var headerTag = await _batchRepository.GetTd3HeaderTag(batchNo, gw.IpAddress);

        if (headerTag is int header)
        {
            await _display.ClearHeader(gw.GatewayId, header);
        }

        PtlLog.Info($"Phase 1 Batch {batchNo} completed");

    }

    // =============================
    // PHASE 2 CONFIRM
    // =============================
    private async Task HandlePhase2ConfirmAsync(
        GatewayRuntimeInfo gw,
        int tag)
    {
        if (!_tagRegistry.TryGet(tag, out var state))
            return;

        PtlLog.Rx($"Confirming Data Tag {tag}");

        int finalQty = state.Quantity;

        var result = await _batchRepository.ConfirmPick(gw.TabelAwal, tag, finalQty, gw.IpAddress); //phase 2 logic transaction

        if (result == null)
            return;

        _tagRegistry.Remove(tag);

        if (!result.SkuCompleted)
        {
            PtlLog.Info($"SKU {result.Plu} still active");
            return;
        }

        var header = await _batchRepository.GetTd0Header(result.BatchNo, result.Plu, gw.IpAddress);

        if (header != null)
        {
            await _display.ClearHeader(gw.GatewayId, header.LokasiPtl);
        }

        PtlLog.Info($"Phase 2 SKU {result.Plu} completed");

        // =============================
        // PHASE 3 INLINE (END BATCH)
        // =============================

        // Check if ALL PLU On batch is finished
        if (await _batchRepository.HasPendingPluOnBatch(result.BatchNo, gw.TabelAwal))
        {
            PtlLog.Info($"Batch {result.BatchNo} still has PLU pending");
            return;
        }

        // 🔥 ALL PLU DONE → END BATCH
        var endRow = await _batchRepository.GetTd5Header(result.BatchNo, gw.IpAddress); //get TD5

        if (endRow == null)
            return;

        await _display.ShowHeader(
            gw.GatewayId,
            endRow.LokasiPtl,
            endRow.Keterangan
        );

        PtlLog.Info($"Batch {result.BatchNo} fully completed");
    }
}