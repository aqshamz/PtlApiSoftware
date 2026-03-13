using Dapper;
using Npgsql;
public class PostgresBatchRepository : IBatchRepository
{
    private readonly PostgresConnectionFactory _factory;

    public PostgresBatchRepository(PostgresConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<T> ExecuteInTransaction<T>(Func<NpgsqlConnection, NpgsqlTransaction, Task<T>> action)
    {
        using var conn = await _factory.CreateOpenConnectionAsync();
        using var tx = await conn.BeginTransactionAsync();

        try
        {
            var result = await action(conn, tx);
            await tx.CommitAsync();
            return result;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<int?> GetNextBatchAsync(string tableName, int flagBatch)
    {
        using var conn = await _factory.CreateOpenConnectionAsync();

        var sql = $@"
            select distinct batch_no
            from {tableName}
            where flag_batch = @flagBatch
            order by batch_no asc limit 1";

        return await conn.ExecuteScalarAsync<int?>(sql, new { flagBatch });
    }

    public async Task<int?> GetNextTD3Header(int batchNo, string ipHub)
    {
        using var conn = await _factory.CreateOpenConnectionAsync();

        var sql = $@"
            select lokasi_ptl::int as lokasi_ptl
            from ptl_str_batch
            where batch_no = @BatchNo
            and ip_hub = @IpHub
            and flag_ptl = 'TD3'
            and coalesce(flag_cek,'0') = '0'
            limit 1";

        return await conn.ExecuteScalarAsync<int?>(sql, new { BatchNo = batchNo, IpHub = ipHub });
    }

    public async Task MarkTD3HeaderChecked(int batchNo, string ipHub)
    {
        using var conn = await _factory.CreateOpenConnectionAsync();

        const string sql = """
            update ptl_str_batch
            set flag_cek = '1'
            where batch_no = @BatchNo
            and ip_hub = @IpHub
            and flag_ptl = 'TD3'
        """;

        await conn.ExecuteAsync(sql, new { BatchNo = batchNo, IpHub = ipHub });
    }

    public async Task<IEnumerable<int>> GetPendingTD4Tags(int batchNo, string ipHub)
    {
        using var conn = await _factory.CreateOpenConnectionAsync();

        const string sql = """
        select lokasi_ptl::int
        from ptl_str_batch
        where batch_no = @BatchNo
        and ip_hub = @IpHub
        and flag_ptl = 'TD4'
        and coalesce(flag_cek,'0') = '0'
    """;

        return await conn.QueryAsync<int>(sql, new { BatchNo = batchNo, IpHub = ipHub });
    }

    public async Task MarkTD4Checked(int batchNo, int tag, string flag, string ipHub)
    {
        using var conn = await _factory.CreateOpenConnectionAsync();

        const string sql = """
        update ptl_str_batch
        set flag_cek = @Flag
        where batch_no = @BatchNo
        and flag_ptl = 'TD4'
        and lokasi_ptl = @Tag
        and ip_hub = @IpHub
    """;

        await conn.ExecuteAsync(sql, new { BatchNo = batchNo, Tag = tag, Flag = flag, IpHub = ipHub });
    }

    public async Task<SkuProcessingInfo?> GetNextSkuForPhase2(string tableName)
    {
        using var conn = await _factory.CreateOpenConnectionAsync();

        var sql = $@"
        select batch_no as BatchNo,
               plu as Plu,
               flag_sending as FlagSending
        from {tableName}
        where flag_batch = 1
        and flag_sending < 2
        order by batch_no, nomor asc
        limit 1";

        return await conn.QueryFirstOrDefaultAsync<SkuProcessingInfo>(sql);
    }

    public async Task<Td0?> GetTd0Header(int batchNo, int plu, string ipHub)
    {
        using var conn = await _factory.CreateOpenConnectionAsync();

        const string sql = """
            select descp as Descp,
                   lokasi_ptl::int as LokasiPtl
            from ptl_str_batch
            where batch_no = @BatchNo
            and plu = @Plu
            and flag_ptl = 'TD0'
            and ip_hub = @IpHub
            limit 1
        """;

        return await conn.QueryFirstOrDefaultAsync<Td0>(
            sql,
            new { BatchNo = batchNo, Plu = plu, IpHub = ipHub });
    }

    public async Task MarkTd0Checked(int batchNo, int plu, string ipHub)
    {
        using var conn = await _factory.CreateOpenConnectionAsync();

        const string sql = """
            update ptl_str_batch
            set flag_cek = '1'
            where batch_no = @BatchNo
            and plu = @Plu
            and flag_ptl = 'TD0'
            and ip_hub = @IpHub
        """;

        await conn.ExecuteAsync(sql, new { BatchNo = batchNo, Plu = plu, IpHub = ipHub });
    }

    public async Task<IEnumerable<D0>> GetD0Items(int batchNo, int plu, string ipHub)
    {
        using var conn = await _factory.CreateOpenConnectionAsync();

        const string sql = """
            select lokasi_ptl::int as LokasiPtl,
                   on_picking::int as OnPicking
            from ptl_str_batch
            where batch_no = @BatchNo
            and plu = @Plu
            and flag_ptl = 'D0'
            and ip_hub = @IpHub
        """;

        return await conn.QueryAsync<D0>(
            sql,
            new { BatchNo = batchNo, Plu = plu, IpHub = ipHub });
    }

    public async Task MarkD0AsD1(int batchNo, int plu, int tag, string ipHub)
    {
        using var conn = await _factory.CreateOpenConnectionAsync();

        //flag_cek = '1',
        const string sql = """
            update ptl_str_batch
            set flag_ptl = 'D1'
            where batch_no = @BatchNo
            and plu = @Plu
            and lokasi_ptl = @Tag
            and ip_hub = @IpHub
        """;

        await conn.ExecuteAsync(sql, new { BatchNo = batchNo, Plu = plu, Tag = tag, IpHub = ipHub });
    }

    public async Task MarkSkuSending(int batchNo, int plu, int flag, string tableName)
    {
        using var conn = await _factory.CreateOpenConnectionAsync();

        var sql = $"""
            update {tableName}
            set flag_sending = @Flag
            where batch_no = @BatchNo
            and plu = @Plu
        """;

        await conn.ExecuteAsync(sql, new { BatchNo = batchNo, Plu = plu, Flag = flag });
    }

    public async Task<bool> HasPendingTd4(int batchNo, string ipHub)
    {
        using var conn = await _factory.CreateOpenConnectionAsync();

        const string sql = """
            select count(*)
            from ptl_str_batch
            where batch_no = @BatchNo
            and flag_ptl = 'TD4'
            and coalesce(flag_cek, '0') <> '2'
            and ip_hub = @IpHub
        """;

        var count = await conn.ExecuteScalarAsync<int>(
            sql,
            new { BatchNo = batchNo, IpHub = ipHub });

        return count > 0;
    }

    public async Task UpdateFlagBatchUrut(int batchNo, int flag, string tableName)
    {
        using var conn = await _factory.CreateOpenConnectionAsync();

        var sql = $"""
            update {tableName}
            set flag_batch = @Flag
            where batch_no = @BatchNo
        """;

        await conn.ExecuteAsync(sql, new { BatchNo = batchNo, Flag = flag });
    }

    public async Task<int?> GetTd3HeaderTag(int batchNo, string ipHub)
    {
        using var conn = await _factory.CreateOpenConnectionAsync();

        const string sql = """
        select lokasi_ptl::int
        from ptl_str_batch
        where batch_no = @BatchNo
        and flag_ptl = 'TD3'
        and ip_hub = @IpHub
        limit 1
    """;

        return await conn.ExecuteScalarAsync<int?>(sql, new { BatchNo = batchNo, IpHub = ipHub });
    }

    public async Task<SkuProcessingInfo?> GetCurrentSendingSku(string tableName)
    {
        using var conn = await _factory.CreateOpenConnectionAsync();

        var sql = $"""
            select batch_no as BatchNo,
                   plu as Plu,
                   flag_sending as FlagSending
            from {tableName}
            where flag_batch = 1
            and flag_sending = 1
            order by batch_no, nomor asc
            limit 1
        """;

        return await conn.QueryFirstOrDefaultAsync<SkuProcessingInfo>(sql);
    }

    public async Task MarkD1AsD2(int batchNo, int plu, int tag, int qty, string ipHub)
    {
        using var conn = await _factory.CreateOpenConnectionAsync();

        //flag_cek = '2',
        const string sql = """
            update ptl_str_batch
            set on_shipping = LEAST(on_picking, @Qty),
                flag_ptl = 'D2'
            where batch_no = @BatchNo
            and plu = @Plu
            and lokasi_ptl = @Tag
            and ip_hub = @IpHub
        """;

        await conn.ExecuteAsync(sql, new { BatchNo = batchNo, Plu = plu, Tag = tag, Qty = qty, IpHub = ipHub });
    }

    public async Task<bool> HasPluActivePending(int batchNo, int plu, string ipHub)
    {
        using var conn = await _factory.CreateOpenConnectionAsync();

        const string sql = """
            select count(*)
            from ptl_str_batch
            where batch_no = @BatchNo
            and plu = @Plu
            and flag_ptl = 'D1'
            and ip_hub = @IpHub
        """;

        var count = await conn.ExecuteScalarAsync<int>(
            sql,
            new { BatchNo = batchNo, Plu = plu, IpHub = ipHub });

        return count > 0;
    }

    public async Task<bool> HasPendingPluOnBatch(int batchNo, string tableName)
    {
        using var conn = await _factory.CreateOpenConnectionAsync();

        var sql = $"""
            select count(*)
            from {tableName}
            where batch_no = @BatchNo
            and flag_sending < 2
        """;

        var count = await conn.ExecuteScalarAsync<int>(
            sql,
            new { BatchNo = batchNo });

        return count > 0;
    }

    public async Task<Td5?> GetTd5Header(int batchNo, string ipHub)
    {
        using var conn = await _factory.CreateOpenConnectionAsync();

        const string sql = """
            select lokasi_ptl::int as LokasiPtl, ket as Keterangan
            from ptl_str_batch
            where batch_no = @BatchNo
            and flag_ptl = 'TD5'
            and ip_hub = @IpHub
            limit 1
        """;

        return await conn.QueryFirstOrDefaultAsync<Td5>(
            sql,
            new { BatchNo = batchNo, IpHub = ipHub });
    }

    public async Task<Phase2ConfirmResult?> ConfirmPick( //single transaction process
    string tableName,
    int tag,
    int qty,
    string ipHub)
    {
        return await ExecuteInTransaction(async (conn, tx) =>
        {
            var sku = await conn.QueryFirstOrDefaultAsync<SkuProcessingInfo>(
            $"""
        select batch_no as BatchNo,
               plu as Plu,
               flag_sending as FlagSending
        from {tableName}
        where flag_batch = 1
        and flag_sending = 1
        order by batch_no, nomor asc
        limit 1
        """, transaction: tx);  //get current active sku on ptl

            if (sku == null)
                return null;

            await conn.ExecuteAsync("""
            update ptl_str_batch
            set on_shipping = LEAST(on_picking, @Qty),
                flag_ptl = 'D2'
            where batch_no = @BatchNo
            and plu = @Plu
            and lokasi_ptl = @Tag
            and ip_hub = @IpHub
        """,
            new
            {
                BatchNo = sku.BatchNo,
                Plu = sku.Plu,
                Tag = tag,
                Qty = qty,
                IpHub = ipHub
            }, tx); //update qty on ship + D1 to D2

            var remaining = await conn.ExecuteScalarAsync<int>("""
            select count(*)
            from ptl_str_batch
            where batch_no = @BatchNo
            and plu = @Plu
            and flag_ptl = 'D1'
            and ip_hub = @IpHub
        """,
            new { BatchNo = sku.BatchNo, Plu = sku.Plu, IpHub = ipHub }, tx); //check plu on tag still active

            if (remaining > 0)
            {
                return new Phase2ConfirmResult
                {
                    BatchNo = sku.BatchNo,
                    Plu = sku.Plu,
                    SkuCompleted = false
                };
            }

            await conn.ExecuteAsync($"""
                update {tableName}
                set flag_sending = 2
                where batch_no = @BatchNo
                and plu = @Plu
            """,
            new { BatchNo = sku.BatchNo, Plu = sku.Plu }, tx); //end plu on ptl urut

            return new Phase2ConfirmResult
            {
                BatchNo = sku.BatchNo,
                Plu = sku.Plu,
                SkuCompleted = true
            };
        });
    }

    //=========================== RECOVERY =======================================
    public async Task<bool> Phase1RecoveryPending(string ipHub)
    {
        using var conn = await _factory.CreateOpenConnectionAsync();

        var sql = $"""
            select count(*) from ptl_str_batch 
            where flag_ptl = 'TD4' and flag_cek = '1' 
            and ip_hub = @IpHub
        """;

        var count = await conn.ExecuteScalarAsync<int>(sql, new { IpHub = ipHub });
        return count > 0;
    }

    public async Task<int?> GetNextTD3HeaderRecovery(int batchNo, string ipHub)
    {
        using var conn = await _factory.CreateOpenConnectionAsync();

        var sql = $@"
            select lokasi_ptl::int as lokasi_ptl
            from ptl_str_batch
            where batch_no = @BatchNo
            and ip_hub = @IpHub
            and flag_ptl = 'TD3'
            and coalesce(flag_cek,'1') = '1'
            limit 1";

        return await conn.ExecuteScalarAsync<int?>(sql, new { BatchNo = batchNo, IpHub = ipHub });
    }

    public async Task<IEnumerable<int>> GetPendingAllTD4Tags(int batchNo, string ipHub)
    {
        using var conn = await _factory.CreateOpenConnectionAsync();

        const string sql = """
        select lokasi_ptl::int
        from ptl_str_batch
        where batch_no = @BatchNo
        and ip_hub = @IpHub
        and flag_ptl = 'TD4'
    """;

        return await conn.QueryAsync<int>(sql, new { BatchNo = batchNo, IpHub = ipHub });
    }

    public async Task<IEnumerable<D0>> GetD0D1Items(int batchNo, int plu, string ipHub)
    {
        using var conn = await _factory.CreateOpenConnectionAsync();

        const string sql = """
            select lokasi_ptl::int as LokasiPtl,
                   on_picking::int as OnPicking
            from ptl_str_batch
            where batch_no = @BatchNo
            and plu = @Plu
            and flag_ptl IN ('D0', 'D1')
            and ip_hub = @IpHub
        """;

        return await conn.QueryAsync<D0>(
            sql,
            new { BatchNo = batchNo, Plu = plu, IpHub = ipHub });
    }
}