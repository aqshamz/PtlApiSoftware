public interface IBatchRepository
{
    Task<int?> GetNextBatchAsync(string tableName, int flagBatch);
    Task<int?> GetNextTD3Header(int batchNo, string ipHub);
    Task MarkTD3HeaderChecked(int batchNo, string ipHub);
    Task<IEnumerable<int>> GetPendingTD4Tags(int batchNo, string ipHub);
    Task MarkTD4Checked(int batchNo, int tag, string flag, string ipHub);
    Task<SkuProcessingInfo?> GetNextSkuForPhase2(string tableName);
    Task<Td0?> GetTd0Header(int batchNo, int plu, string ipHub);
    Task MarkTd0Checked(int batchNo, int plu, string ipHub);
    Task<IEnumerable<D0>> GetD0Items(int batchNo, int plu, string ipHub);
    Task MarkD0AsD1(int batchNo, int plu, int tag, string ipHub);
    Task MarkSkuSending(int batchNo, int plu, int flag, string tableName);
    Task<bool> HasPendingTd4(int batchNo, string ipHub);
    Task UpdateFlagBatchUrut(int batchNo, int flag, string tableName);
    Task<int?> GetTd3HeaderTag(int batchNo, string ipHub);
    Task<SkuProcessingInfo?> GetCurrentSendingSku(string tableName);
    Task MarkD1AsD2(int batchNo, int plu, int tag, int qty, string ipHub);
    Task<bool> HasPluActivePending(int batchNo, int plu, string ipHub);
    Task<bool> HasPendingPluOnBatch(int batchNo, string tableName);
    Task<Td5?> GetTd5Header(int batchNo, string ipHub);
    Task<Phase2ConfirmResult?> ConfirmPick(string tableName, int tag, int qty, string ipHub);
    Task<bool> Phase1RecoveryPending(string ipHub);
    Task<int?> GetNextTD3HeaderRecovery(int batchNo, string ipHub);
    Task<IEnumerable<int>> GetPendingAllTD4Tags(int batchNo, string ipHub);
    Task<IEnumerable<D0>> GetD0D1Items(int batchNo, int plu, string ipHub);
}