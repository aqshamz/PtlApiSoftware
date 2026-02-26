using Dapper;
using Npgsql;
using Ptl.Agent.Domain;
using Ptl.Contracts.Dtos.Hardware;

public class BatchEngineService
{
    private readonly ConnectedGatewayRegistry _registry;
    private readonly Phase2TagRegistry _tagRegistry;
    private readonly IConfiguration _cfg;
    private readonly IHttpClientFactory _httpFactory;

    public BatchEngineService(
        ConnectedGatewayRegistry registry,
        Phase2TagRegistry tagRegistry,
        IConfiguration cfg,
        IHttpClientFactory httpFactory)
    {
        _registry = registry;
        _cfg = cfg;
        _tagRegistry = tagRegistry;
        _httpFactory = httpFactory;
    }

    public async Task ProcessAsync()
    {
        var gateways = _registry.GetConnected().ToList();
        if (!gateways.Any()) return;

        foreach (var gw in gateways)
        {
            var cs = _cfg.GetConnectionString("PgDb");
            using var conn = new NpgsqlConnection(cs);
            await conn.OpenAsync();

            await HandlePhase1(conn, gw);
            await HandlePhase2(conn, gw);
        }
    }

    // =============================
    // PHASE 1 (flag_batch = 4)
    // =============================
    private async Task HandlePhase1(NpgsqlConnection conn, GatewayRuntimeInfo gw)
    {
        var batchNo = await conn.ExecuteScalarAsync<int?>($"""
            select distinct batch_no
            from {gw.TabelAwal}
            where flag_batch = 4
            limit 1
        """);

        if (batchNo == null)
            return;

        // Send Header
        var header = await conn.QueryFirstOrDefaultAsync<dynamic>("""
            select lokasi_ptl::int as lokasi_ptl
            from ptl_str_batch
            where batch_no = @BatchNo
            and flag_ptl = 'TD3'
            and coalesce(flag_cek,'0') = '0'
            limit 1
        """, new { BatchNo = batchNo });

        if (header != null)
        {
            await SendToHardware(new PtlTxCommandDto
            {
                Gateway = gw.GatewayId,
                Tag = header.lokasi_ptl,
                ClearHeader = true
            });

            await SendToHardware(new PtlTxCommandDto
            {
                Gateway = gw.GatewayId,
                Tag = header.lokasi_ptl,
                Text = $"BATCH - {batchNo}"
            });

            await conn.ExecuteAsync("""
                update ptl_str_batch
                set flag_cek = '1'
                where batch_no = @BatchNo
                and flag_ptl = 'TD3'
            """, new { BatchNo = batchNo });
        }

        // Send TD4 tags
        var rows = await conn.QueryAsync<dynamic>("""
            select lokasi_ptl::int as lokasi_ptl
            from ptl_str_batch
            where batch_no = @BatchNo
            and flag_ptl = 'TD4'
            and coalesce(flag_cek,'0') = '0'
        """, new { BatchNo = batchNo });

        foreach (var row in rows)
        {
            await SendToHardware(new PtlTxCommandDto
            {
                Gateway = gw.GatewayId,
                Tag = row.lokasi_ptl,
                Qty = row.lokasi_ptl
            });

            await conn.ExecuteAsync("""
                update ptl_str_batch
                set flag_cek = '1'
                where batch_no = @BatchNo
                and flag_ptl = 'TD4'
                and lokasi_ptl = @Tag
            """, new { BatchNo = batchNo, Tag = row.lokasi_ptl });
        }
    }

    // =============================
    // PHASE 2 (flag_batch = 1)
    // =============================
    private async Task HandlePhase2(NpgsqlConnection conn, GatewayRuntimeInfo gw)
    {
        var currentSku = await conn.QueryFirstOrDefaultAsync<dynamic>($"""
            select batch_no, plu, flag_sending
            from {gw.TabelAwal}
            where flag_batch = 1
            and flag_sending < 2
            order by nomor asc
            limit 1
        """);

        if (currentSku == null)
            return;

        int batchNo = (int)currentSku.batch_no;
        int plu = (int)currentSku.plu;
        int flagSending = (int)currentSku.flag_sending;

        if (flagSending == 0)
        {
            await LoadSku(conn, gw, batchNo, plu);
            return;
        }

        //await HandlePhase3(conn, gw, batchNo);
    }

    private async Task LoadSku(
        NpgsqlConnection conn,
        GatewayRuntimeInfo gw,
        int batchNo,
        int plu)
    {
        // TD0 Header
        var header = await conn.QueryFirstOrDefaultAsync<dynamic>("""
            select descp, lokasi_ptl::int as lokasi_ptl
            from ptl_str_batch
            where batch_no = @BatchNo
            and plu = @Plu
            and flag_ptl = 'TD0'
            limit 1
        """, new { BatchNo = batchNo, Plu = plu });

        if (header != null)
        {
            await SendToHardware(new PtlTxCommandDto
            {
                Gateway = gw.GatewayId,
                Tag = header.lokasi_ptl,
                ClearHeader = true
            });

            await SendToHardware(new PtlTxCommandDto
            {
                Gateway = gw.GatewayId,
                Tag = header.lokasi_ptl,
                Text = header.descp
            });

            await conn.ExecuteAsync("""
                update ptl_str_batch
                set flag_cek = '1'
                where batch_no = @BatchNo
                and plu = @Plu
                and flag_ptl = 'TD0'
            """, new { BatchNo = batchNo, Plu = plu });
        }

        // D0 → D1
        var rows = await conn.QueryAsync<dynamic>("""
            select lokasi_ptl::int as lokasi_ptl,
                   on_picking::int as on_picking
            from ptl_str_batch
            where batch_no = @BatchNo
            and plu = @Plu
            and flag_ptl = 'D0'
        """, new { BatchNo = batchNo, Plu = plu });

        foreach (var row in rows)
        {
            await SendToHardware(new PtlTxCommandDto
            {
                Gateway = gw.GatewayId,
                Tag = row.lokasi_ptl,
                Qty = row.on_picking
            });

            var state = new TagState(
                gw.GatewayId,
                row.lokasi_ptl,
                row.on_picking,
                $"{batchNo}-{plu}"
            );

            _tagRegistry.Set(state);

            //flag_cek = '1'
            await conn.ExecuteAsync("""
                update ptl_str_batch
                set flag_ptl = 'D1'
                where batch_no = @BatchNo
                and plu = @Plu
                and lokasi_ptl = @Tag
            """, new { BatchNo = batchNo, Plu = plu, Tag = row.lokasi_ptl });
        }

        await conn.ExecuteAsync($"""
            update {gw.TabelAwal}
            set flag_sending = 1
            where batch_no = @BatchNo
            and plu = @Plu
        """, new { BatchNo = batchNo, Plu = plu });
    }

    // =============================
    // PHASE 3 (END BATCH)
    // =============================
    private async Task HandlePhase3(
    NpgsqlConnection conn,
    GatewayRuntimeInfo gw,
    int batchNo)
    {
        var remaining = await conn.ExecuteScalarAsync<int>($"""
            select count(*)
            from {gw.TabelAwal}
            where batch_no = @BatchNo
            and flag_batch = 1
            and flag_sending <> 2
        """, new { BatchNo = batchNo });

        if (remaining > 0)
            return;

        // All SKU finished → send END BATCH
        var endRow = await conn.QueryFirstOrDefaultAsync<dynamic>("""
            select lokasi_ptl::int as lokasi_ptl, ket
            from ptl_str_batch
            where batch_no = @BatchNo
            and flag_ptl = 'TD5'
            limit 1
        """, new { BatchNo = batchNo });

        if (endRow == null)
            return;

        await SendToHardware(new PtlTxCommandDto
        {
            Gateway = gw.GatewayId,
            Tag = endRow.lokasi_ptl,
            Text = endRow.ket
        });

        // Optional but IMPORTANT to prevent retrigger loop
        await conn.ExecuteAsync($"""
            update {gw.TabelAwal}
            set flag_batch = 2
            where batch_no = @BatchNo
        """, new { BatchNo = batchNo });

        Console.WriteLine($"[PH3] Batch {batchNo} ended");
    }

    private async Task SendToHardware(PtlTxCommandDto dto)
    {
        var client = _httpFactory.CreateClient("hardware");
        await client.PostAsJsonAsync("/ptl/execute", dto);
    }
}