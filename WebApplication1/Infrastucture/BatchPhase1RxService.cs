using Dapper;
using Npgsql;
using Ptl.Agent.Domain;
using Ptl.Contracts.Dtos.Hardware;

public class BatchPhase1RxService
{
    private readonly ConnectedGatewayRegistry _registry;
    private readonly Phase2TagRegistry _tagRegistry;
    private readonly IConfiguration _cfg;
    private readonly IPtlDisplay _display;

    public BatchPhase1RxService(
        ConnectedGatewayRegistry registry,
        Phase2TagRegistry tagRegistry,
        IPtlDisplay display,
        IConfiguration cfg)
    {
        _registry = registry;
        _tagRegistry = tagRegistry;
        _display = display;
        _cfg = cfg;
    }

    public async Task HandleAsync(PtlRxEventDto evt)
    {
        var gw = _registry.GetConnected()
            .FirstOrDefault(g => g.GatewayId == evt.Gateway);

        if (gw == null)
            return;

        var cs = _cfg.GetConnectionString("PgDb");
        using var conn = new NpgsqlConnection(cs);
        await conn.OpenAsync();

        // Phase 1 (batch 4)
        await HandlePhase1Async(conn, gw, evt.Tag);

        Console.WriteLine($"[RX] CMD={evt.Command} TAG={evt.Tag}");

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

            Console.WriteLine($"[PH2] Decrease tag={evt.Tag} qty={state.Quantity}");
            return;
        }

        // Phase 2 Confirm
        if (evt.Command == PtlCommand.Confirm)
        {
            await HandlePhase2ConfirmAsync(conn, gw, evt.Tag);
        }
    }

    // =============================
    // PHASE 1
    // =============================
    private async Task HandlePhase1Async(
        NpgsqlConnection conn,
        GatewayRuntimeInfo gw,
        int tag)
    {
        var batchNo = await conn.ExecuteScalarAsync<int?>($"""
            select distinct batch_no
            from {gw.TabelAwal}
            where flag_batch = 4
            limit 1
        """);

        if (batchNo == null)
            return;

        // Update TD4 clicked row
        await conn.ExecuteAsync("""
            update ptl_str_batch
            set flag_cek = '2'
            where batch_no = @BatchNo
            and flag_ptl = 'TD4'
            and lokasi_ptl = @Tag
        """,
        new { BatchNo = batchNo, Tag = tag });

        var remaining = await conn.ExecuteScalarAsync<int>("""
            select count(*)
            from ptl_str_batch
            where batch_no = @BatchNo
            and flag_ptl = 'TD4'
            and coalesce(flag_cek, '0') <> '2'
        """,
        new { BatchNo = batchNo });

        if (remaining > 0)
            return;

        // Mark batch 0
        await conn.ExecuteAsync($"""
            update {gw.TabelAwal}
            set flag_batch = 0
            where batch_no = @BatchNo
        """, new { BatchNo = batchNo });

        // Clear header
        var headerTag = await conn.ExecuteScalarAsync<int?>("""
            select lokasi_ptl::int
            from ptl_str_batch
            where batch_no = @BatchNo
            and flag_ptl = 'TD3'
            limit 1
        """,
        new { BatchNo = batchNo });

        if (headerTag != null)
        {
            await _display.ClearHeader(gw.GatewayId, headerTag.Value);
        }

        Console.WriteLine($"[PH1] Batch {batchNo} completed");
    }

    // =============================
    // PHASE 2 CONFIRM
    // =============================
    private async Task HandlePhase2ConfirmAsync(
        NpgsqlConnection conn,
        GatewayRuntimeInfo gw,
        int tag)
    {
        if (!_tagRegistry.TryGet(tag, out var state))
            return;

        int finalQty = state.Quantity;

        var skuRow = await conn.QueryFirstOrDefaultAsync<dynamic>($"""
            select batch_no, plu
            from {gw.TabelAwal}
            where flag_batch = 1
            and flag_sending = 1
            order by nomor asc
            limit 1
        """);

        if (skuRow == null)
            return;

        int batchNo = (int)skuRow.batch_no;
        int plu = (int)skuRow.plu;

        //flag_cek = '2',
        await conn.ExecuteAsync("""
            update ptl_str_batch
            set on_shipping = LEAST(on_picking, @Qty),
                flag_ptl = 'D2'
            where batch_no = @BatchNo
            and plu = @Plu
            and lokasi_ptl = @Tag
        """,
        new
        {
            BatchNo = batchNo,
            Plu = plu,
            Tag = tag,
            Qty = finalQty
        });

        _tagRegistry.Remove(tag);

        var remaining = await conn.ExecuteScalarAsync<int>("""
            select count(*)
            from ptl_str_batch
            where batch_no = @BatchNo
            and plu = @Plu
            and flag_ptl = 'D1'
        """,
        new { BatchNo = batchNo, Plu = plu });

        if (remaining > 0)
        {
           Console.WriteLine($"[PH2] SKU {plu} still has {remaining} tags pending");
           return;
        }

            // 3️⃣ Now ALL tags done → mark flag_sending = 2
        await conn.ExecuteAsync($"""
            update {gw.TabelAwal}
            set flag_sending = 2
            where batch_no = @BatchNo
            and plu = @Plu
        """,
        new { BatchNo = batchNo, Plu = plu });

        var headerTag = await conn.ExecuteScalarAsync<int?>("""
            select lokasi_ptl::int
            from ptl_str_batch
            where batch_no = @BatchNo
            and flag_ptl = 'TD0'
            limit 1
        """,
        new { BatchNo = batchNo });

        if (headerTag != null)
        {
            await _display.ClearHeader(gw.GatewayId, headerTag.Value);
        }

        Console.WriteLine($"[PH2] SKU {plu} fully completed");

        // =============================
        // PHASE 3 INLINE (END BATCH)
        // =============================

        // Check if ALL PLU finished
        var batchRemaining = await conn.ExecuteScalarAsync<int>($"""
            select count(*)
            from {gw.TabelAwal}
            where batch_no = @BatchNo
            and flag_batch = 1
            and flag_sending <> 2
        """,
        new { BatchNo = batchNo });

        if (batchRemaining > 0)
        {
            Console.WriteLine($"[PH3] Batch {batchNo} still has {batchRemaining} PLU pending");
            return;
        }

        // 🔥 ALL PLU DONE → END BATCH
        var endRow = await conn.QueryFirstOrDefaultAsync<dynamic>("""
            select lokasi_ptl::int as lokasi_ptl, ket
            from ptl_str_batch
            where batch_no = @BatchNo
            and flag_ptl = 'TD5'
            limit 1
        """,
        new { BatchNo = batchNo });

        if (endRow == null)
            return;

        await _display.ShowHeader(
            gw.GatewayId,
            endRow.lokasi_ptl,
            endRow.ket
        );

        //// Prevent retrigger
        //await conn.ExecuteAsync($"""
        //    update {gw.TabelAwal}
        //    set flag_batch = 2
        //    where batch_no = @BatchNo
        //""",
        //new { BatchNo = batchNo });

        Console.WriteLine($"[PH3] Batch {batchNo} fully completed");
    }
}