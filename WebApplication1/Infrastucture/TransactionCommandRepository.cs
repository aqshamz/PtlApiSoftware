using Dapper;
using MySqlConnector;
using Ptl.Contracts.Dtos;
using Ptl.Core.Interfaces;

public class TransactionCommandRepository : ITransactionCommandRepository
{
    private readonly string _cs;

    public TransactionCommandRepository(IConfiguration config)
    {
        _cs = config.GetConnectionString("PtlDb");
    }

    public void InsertTransaction(PickTransactionDto tx)
    {
        using var conn = new MySqlConnection(_cs);
        conn.Open();

        using var trx = conn.BeginTransaction();

        try
        {
            const string insertHeader = """
                INSERT INTO transaction
                    (id, header_gateaway, header_address, header_text, status)
                VALUES
                    (@TxId, @HeaderGateaway, @HeaderTag, @HeaderText, 0);
            """;

            conn.Execute(insertHeader, tx, trx);

            const string insertDetail = """
                INSERT INTO transaction_detail
                    (id, transaction_id, gateaway, address, qty, sku, status_picked)
                VALUES
                    (@TxDetailId, @TxId, @Gateaway, @Tag, @Qty, @Sku, 0);
            """;

            foreach (var d in tx.DataDetail)
            {

                var detailId = GenerateDetailId(tx.TxId);

                conn.Execute(insertDetail, new
                {
                    TxDetailId = detailId,
                    TxId = tx.TxId,
                    d.Gateaway,
                    d.Tag,
                    d.Qty,
                    d.Sku
                }, trx);
            }

            trx.Commit();
        }
        catch
        {
            trx.Rollback();
            throw;
        }
    }

    private static string GenerateDetailId(string txId)
    {
        return $"{txId}-{Guid.NewGuid():N}".Substring(0, txId.Length + 1 + 8);
    }
}
