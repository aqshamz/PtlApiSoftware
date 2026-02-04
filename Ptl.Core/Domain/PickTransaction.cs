using System;
using System.Collections.Generic;

namespace Ptl.Agent.Domain
{
    public class PickTransaction
    {
        public string TransactionId { get; set; } = string.Empty;

        // Header PTL
        public int HeaderTag { get; set; }
        public string HeaderText { get; set; } = string.Empty;

        public int HeaderGetaway { get; set; }

        // tagAddress -> quantity
        //public Dictionary<int, int> Items { get; } = new();
        //public Dictionary<int, string> TxDetailId { get; } = new();

        public HashSet<int> ActiveTags { get; } = new();

        public DateTime CreatedAt { get; } = DateTime.UtcNow;

        public bool IsStarted { get; set; } // 🔥 ADD
        public bool IsCompleted => ActiveTags.Count == 0;
    }
}
