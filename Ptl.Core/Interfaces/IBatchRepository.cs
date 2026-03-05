public interface IBatchRepository
{
    Task<int?> GetNextBatchAsync(string tableName, int flagBatch);
    Task<int?> GetNextTD3Header(int batchNo);
    Task MarkTD3HeaderChecked(int batchNo);
    Task<IEnumerable<int>> GetPendingTD4Tags(int batchNo);
    Task MarkTD4Checked(int batchNo, int tag, string flag);
    Task<SkuProcessingInfo?> GetNextSkuForPhase2(string tableName);
    Task<Td0?> GetTd0Header(int batchNo, int plu);
    Task MarkTd0Checked(int batchNo, int plu);
    Task<IEnumerable<D0>> GetD0Items(int batchNo, int plu);
    Task MarkD0AsD1(int batchNo, int plu, int tag);
    Task MarkSkuSending(int batchNo, int plu, int flag, string tableName);
    Task<bool> HasPendingTd4(int batchNo);
    Task UpdateFlagBatchUrut(int batchNo, int flag, string tableName);
    Task<int?> GetTd3HeaderTag(int batchNo);
    Task<SkuProcessingInfo?> GetCurrentSendingSku(string tableName);
    Task MarkD1AsD2(int batchNo, int plu, int tag, int qty);
    Task<bool> HasPluActivePending(int batchNo, int plu);
    Task<bool> HasPendingPluOnBatch(int batchNo, string tableName);
    Task<Td5?> GetTd5Header(int batchNo);
    Task<Phase2ConfirmResult?> ConfirmPick(string tableName, int tag, int qty);
}