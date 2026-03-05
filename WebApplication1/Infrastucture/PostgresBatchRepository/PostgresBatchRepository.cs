using Dapper;
using Npgsql;
using Ptl.Core.Interfaces;

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
            limit 1";

        return await conn.ExecuteScalarAsync<int?>(sql, new { flagBatch });
    }

    public async Task<int?> GetNextTD3Header(int batchNo)
    {
        using var conn = await _factory.CreateOpenConnectionAsync();

        var sql = $@"
            select lokasi_ptl::int as lokasi_ptl
            from ptl_str_batch
            where batch_no = @batchNo
            and flag_ptl = 'TD3'
            and coalesce(flag_cek,'0') = '0'
            limit 1";

        return await conn.ExecuteScalarAsync<int?>(sql, new { batchNo });
    }

    public async Task MarkTD3HeaderChecked(int batchNo)
    {
        using var conn = await _factory.CreateOpenConnectionAsync();

        const string sql = """
            update ptl_str_batch
            set flag_cek = '1'
            where batch_no = @BatchNo
            and flag_ptl = 'TD3'
        """;

        await conn.ExecuteAsync(sql, new { BatchNo = batchNo });
    }

    public async Task<IEnumerable<int>> GetPendingTD4Tags(int batchNo)
    {
        using var conn = await _factory.CreateOpenConnectionAsync();

        const string sql = """
        select lokasi_ptl::int
        from ptl_str_batch
        where batch_no = @BatchNo
        and flag_ptl = 'TD4'
        and coalesce(flag_cek,'0') = '0'
    """;

        return await conn.QueryAsync<int>(sql, new { BatchNo = batchNo });
    }

    public async Task MarkTD4Checked(int batchNo, int tag, string flag)
    {
        using var conn = await _factory.CreateOpenConnectionAsync();

        const string sql = """
        update ptl_str_batch
        set flag_cek = @Flag
        where batch_no = @BatchNo
        and flag_ptl = 'TD4'
        and lokasi_ptl = @Tag
    """;

        await conn.ExecuteAsync(sql, new { BatchNo = batchNo, Tag = tag, Flag = flag });
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
        order by nomor asc
        limit 1";

        return await conn.QueryFirstOrDefaultAsync<SkuProcessingInfo>(sql);
    }

    public async Task<Td0?> GetTd0Header(int batchNo, int plu)
    {
        using var conn = await _factory.CreateOpenConnectionAsync();

        const string sql = """
            select descp as Descp,
                   lokasi_ptl::int as LokasiPtl
            from ptl_str_batch
            where batch_no = @BatchNo
            and plu = @Plu
            and flag_ptl = 'TD0'
            limit 1
        """;

        return await conn.QueryFirstOrDefaultAsync<Td0>(
            sql,
            new { BatchNo = batchNo, Plu = plu });
    }

    public async Task MarkTd0Checked(int batchNo, int plu)
    {
        using var conn = await _factory.CreateOpenConnectionAsync();

        const string sql = """
            update ptl_str_batch
            set flag_cek = '1'
            where batch_no = @BatchNo
            and plu = @Plu
            and flag_ptl = 'TD0'
        """;

        await conn.ExecuteAsync(sql, new { BatchNo = batchNo, Plu = plu });
    }

    public async Task<IEnumerable<D0>> GetD0Items(int batchNo, int plu)
    {
        using var conn = await _factory.CreateOpenConnectionAsync();

        const string sql = """
            select lokasi_ptl::int as LokasiPtl,
                   on_picking::int as OnPicking
            from ptl_str_batch
            where batch_no = @BatchNo
            and plu = @Plu
            and flag_ptl = 'D0'
        """;

        return await conn.QueryAsync<D0>(
            sql,
            new { BatchNo = batchNo, Plu = plu });
    }

    public async Task MarkD0AsD1(int batchNo, int plu, int tag)
    {
        using var conn = await _factory.CreateOpenConnectionAsync();

        //flag_cek = '1',
        const string sql = """
            update ptl_str_batch
            set flag_ptl = 'D1'
            where batch_no = @BatchNo
            and plu = @Plu
            and lokasi_ptl = @Tag
        """;

        await conn.ExecuteAsync(sql, new { BatchNo = batchNo, Plu = plu, Tag = tag });
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

    public async Task<bool> HasPendingTd4(int batchNo)
    {
        using var conn = await _factory.CreateOpenConnectionAsync();

        const string sql = """
            select count(*)
            from ptl_str_batch
            where batch_no = @BatchNo
            and flag_ptl = 'TD4'
            and coalesce(flag_cek, '0') <> '2'
        """;

        var count = await conn.ExecuteScalarAsync<int>(
            sql,
            new { BatchNo = batchNo });

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

    public async Task<int?> GetTd3HeaderTag(int batchNo)
    {
        using var conn = await _factory.CreateOpenConnectionAsync();

        const string sql = """
        select lokasi_ptl::int
        from ptl_str_batch
        where batch_no = @BatchNo
        and flag_ptl = 'TD3'
        limit 1
    """;

        return await conn.ExecuteScalarAsync<int?>(sql, new { BatchNo = batchNo });
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
            order by nomor asc
            limit 1
        """;

        return await conn.QueryFirstOrDefaultAsync<SkuProcessingInfo>(sql);
    }

    public async Task MarkD1AsD2(int batchNo, int plu, int tag, int qty)
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
        """;

        await conn.ExecuteAsync(sql, new { BatchNo = batchNo, Plu = plu, Tag = tag, Qty = qty });
    }

    public async Task<bool> HasPluActivePending(int batchNo, int plu)
    {
        using var conn = await _factory.CreateOpenConnectionAsync();

        const string sql = """
            select count(*)
            from ptl_str_batch
            where batch_no = @BatchNo
            and plu = @Plu
            and flag_ptl = 'D1'
        """;

        var count = await conn.ExecuteScalarAsync<int>(
            sql,
            new { BatchNo = batchNo, Plu = plu });

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

    public async Task<Td5?> GetTd5Header(int batchNo)
    {
        using var conn = await _factory.CreateOpenConnectionAsync();

        const string sql = """
            select lokasi_ptl::int as LokasiPtl, ket as Keterangan
            from ptl_str_batch
            where batch_no = @BatchNo
            and flag_ptl = 'TD5'
            limit 1
        """;

        return await conn.QueryFirstOrDefaultAsync<Td5>(
            sql,
            new { BatchNo = batchNo });
    }

    public async Task<Phase2ConfirmResult?> ConfirmPick( //single transaction process
    string tableName,
    int tag,
    int qty)
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
        order by nomor asc
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
        """,
            new
            {
                BatchNo = sku.BatchNo,
                Plu = sku.Plu,
                Tag = tag,
                Qty = qty
            }, tx); //update qty on ship + D1 to D2

            var remaining = await conn.ExecuteScalarAsync<int>("""
            select count(*)
            from ptl_str_batch
            where batch_no = @BatchNo
            and plu = @Plu
            and flag_ptl = 'D1'
        """,
            new { BatchNo = sku.BatchNo, Plu = sku.Plu }, tx); //check plu on tag still active

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
}