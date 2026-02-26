using Dapper;
using Npgsql;
using Ptl.Contracts.Dtos.Hardware;
using System.Net.Http.Json;

public class BatchPhase1Service
{
    private readonly ConnectedGatewayRegistry _registry;
    private readonly IConfiguration _cfg;
    private readonly IHttpClientFactory _httpFactory;

    public BatchPhase1Service(
        ConnectedGatewayRegistry registry,
        IConfiguration cfg,
        IHttpClientFactory httpFactory)
    {
        _registry = registry;
        _cfg = cfg;
        _httpFactory = httpFactory;
    }

    public async Task ProcessAsync()
    {
        var connected = _registry.GetConnected().ToList();
        if (!connected.Any())
            return;

        var cs = _cfg.GetConnectionString("PgDb");
        using var conn = new NpgsqlConnection(cs);
        await conn.OpenAsync();

        foreach (var gw in connected)
        {
            // 1️⃣ Get active batch
            var batchNo = await conn.ExecuteScalarAsync<int?>($"""
                select distinct batch_no
                from {gw.TabelAwal}
                where flag_batch = 4
                limit 1
            """);

            if (batchNo == null)
                continue;

            Console.WriteLine($"[PH1] Gateway {gw.GatewayId} active batch {batchNo}");

            await SendHeaderAsync(conn, gw, batchNo.Value);
            await SendTagNumbersAsync(conn, gw, batchNo.Value);
        }
    }

    private async Task SendHeaderAsync(
        NpgsqlConnection conn,
        GatewayRuntimeInfo gw,
        int batchNo)
    {
        var row = await conn.QueryFirstOrDefaultAsync<dynamic>(
            """
            select lokasi_ptl::int as lokasi_ptl
            from ptl_str_batch
            where batch_no = @BatchNo
            and flag_ptl = 'TD3'
            and (flag_cek is null or flag_cek = '0')
            limit 1
            """,
            new { BatchNo = batchNo });

        if (row == null)
            return;

        int tag = row.lokasi_ptl;

        var dto = new PtlTxCommandDto
        {
            Gateway = gw.GatewayId,
            Tag = tag,
            Text = $"BATCH - {batchNo}"
        };

        await SendToHardware(dto);

        await conn.ExecuteAsync(
            """
            update ptl_str_batch
            set flag_cek = '1'
            where batch_no = @BatchNo
            and flag_ptl = 'TD3'
            """,
            new { BatchNo = batchNo });

        Console.WriteLine($"[PH1] Header sent to tag {tag}");
    }

    private async Task SendTagNumbersAsync(
        NpgsqlConnection conn,
        GatewayRuntimeInfo gw,
        int batchNo)
    {
        var rows = await conn.QueryAsync<dynamic>(
            """
            select lokasi_ptl::int as lokasi_ptl
            from ptl_str_batch
            where batch_no = @BatchNo
            and flag_ptl = 'TD4'
            and (flag_cek is null or flag_cek = '0')
            """,
            new { BatchNo = batchNo });

        foreach (var row in rows)
        {
            int tag = row.lokasi_ptl;

            var dto = new PtlTxCommandDto
            {
                Gateway = gw.GatewayId,
                Tag = tag,
                Qty = tag // show tag number as display
            };

            await SendToHardware(dto);

            await conn.ExecuteAsync(
                """
                update ptl_str_batch
                set flag_cek = '1'
                where batch_no = @BatchNo
                and flag_ptl = 'TD4'
                and lokasi_ptl = @Tag
                """,
                new { BatchNo = batchNo, Tag = tag });

            Console.WriteLine($"[PH1] Number sent to tag {tag}");
        }
    }

    private async Task SendToHardware(PtlTxCommandDto dto)
    {
        var client = _httpFactory.CreateClient("hardware");

        await client.PostAsJsonAsync("/ptl/execute", dto);
    }
}