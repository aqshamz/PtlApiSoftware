using System.Collections.Generic;

namespace Ptl.Contracts.Dtos
{
    public class PickTransactionDto
    {
        public string TxId { get; set; } = default!;
        public int HeaderGateaway { get; set; }
        public int HeaderTag { get; set; }
        public string HeaderText { get; set; } = default!;
        public List<PickTransactionDetailDto> DataDetail { get; set; } = new();
    }
}
