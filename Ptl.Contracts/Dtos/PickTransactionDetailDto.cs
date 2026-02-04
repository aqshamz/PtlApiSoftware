namespace Ptl.Contracts.Dtos
{
    public class PickTransactionDetailDto
    {
        public int Gateaway { get; set; }
        public int Tag { get; set; }
        public int Qty { get; set; }
        public string TxDetailId { get; set; } = default!;
    }
}
