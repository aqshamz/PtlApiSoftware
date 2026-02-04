using Dapper;
using MySqlConnector;
using Ptl.Contracts.Dtos;
using Ptl.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Ptl.Api.Infrastructure.Repositories;

public class MySqlTransactionSource : ITransactionSource
{
    private readonly string _cs;

    public MySqlTransactionSource(IConfiguration config)
    {
        _cs = config.GetConnectionString("PtlDb")!;
    }

    public PickTransactionDto? GetNextTransaction()
    {
        using var conn = new MySqlConnection(_cs);

        // 🔹 TEMP: hardcoded query (we’ll refine)
        const string sqlHeader = """
            SELECT
                id       AS TxId,
                header_gateaway AS HeaderGateaway,
                header_address     AS HeaderTag,
                header_text    AS HeaderText
            FROM transaction
            WHERE status = 0
            ORDER BY created_date
            LIMIT 1;
        """;

        var header = conn.QueryFirstOrDefault<PickTransactionDto>(sqlHeader);
        if (header == null)
            return null;

        const string sqlDetail = """
            SELECT
                id          AS TxDetailId,
                gateaway     AS Gateaway,
                address     AS Tag,
                qty         AS Qty
            FROM transaction_detail
            WHERE transaction_id = @TxId
            AND status_picked = 0;
        """;

        var details = conn.Query<PickTransactionDetailDto>(
            sqlDetail,
            new { header.TxId }
        );

        header.DataDetail.AddRange(details);

        //Console.WriteLine($"[DB] Loaded TX {header.TxId}");

        return header;
    }

    public IEnumerable<PickTransactionDto> GetActiveTransactions()
    {
        using var conn = new MySqlConnection(_cs);

        const string sqlHeader = """
            SELECT
                id       AS TxId,
                header_gateaway AS HeaderGateaway,
                header_address  AS HeaderTag,
                header_text     AS HeaderText
            FROM transaction
            WHERE status = 1
            ORDER BY created_date;
        """;

        var headers = conn.Query<PickTransactionDto>(sqlHeader).ToList();

        const string sqlDetail = """
        SELECT
            id          AS TxDetailId,
            gateaway    AS Gateaway,
            address     AS Tag,
            qty         AS Qty
        FROM transaction_detail
        WHERE transaction_id = @TxId
          AND status_picked = 0;
    """;

        foreach (var header in headers)
        {
            var details = conn.Query<PickTransactionDetailDto>(
                sqlDetail,
                new { header.TxId }
            );

            header.DataDetail.AddRange(details);
        }

        return headers;
    }

    public bool UpdateTransaction(string txId, int status)
    {
        using var conn = new MySqlConnection(_cs);

        var rows = conn.Execute(
            "UPDATE transaction SET status=@status WHERE id=@txId",
            new { txId, status }
        );

        return rows == 1;
    }

    public bool ProcessPicked(string txDetailId, int qty)
    {
       using var conn = new MySqlConnection(_cs);

       var rows = conn.Execute("""
            UPDATE transaction_detail
            SET qty_picked = @qty,
            status_picked = 1
            WHERE id = @txDetailId
        """, new { txDetailId, qty });

       return rows == 1;
    }
}
