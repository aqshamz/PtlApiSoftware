using System.Collections.Generic;

namespace Ptl.Contracts.Events
{
    public class PickConfirmedEvent
    {
        public string EventId { get; init; } = Guid.NewGuid().ToString();
        public string TxId { get; init; } = default!;
        public string TxDetailId { get; init; } = default!;
        public int PickedQty { get; init; }
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

        public bool Processed { get; set; } = false;
    }
}
