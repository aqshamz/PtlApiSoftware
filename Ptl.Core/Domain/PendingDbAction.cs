using System;

namespace Ptl.Agent.Domain
{
    public enum PendingDbActionType
    {
        PickDetail,
        CompleteTransaction
    }

    public class PendingDbAction
    {
        public PendingDbActionType ActionType { get; init; }

        // For PickDetail
        public string? TxDetailId { get; init; }
        public int Qty { get; init; }

        // For CompleteTransaction
        public string? TransactionId { get; init; }

        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

        public static PendingDbAction ForDetail(string txDetailId, int qty)
       => new()
       {
           ActionType = PendingDbActionType.PickDetail,
           TxDetailId = txDetailId,
           Qty = qty
       };

        public static PendingDbAction ForTransaction(string txId)
            => new()
            {
                ActionType = PendingDbActionType.CompleteTransaction,
                TransactionId = txId
            };
    }
}
