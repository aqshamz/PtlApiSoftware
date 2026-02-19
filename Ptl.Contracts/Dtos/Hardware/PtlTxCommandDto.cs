namespace Ptl.Contracts.Dtos.Hardware
{
    public class PtlTxCommandDto
    {
        public int Gateway { get; set; }
        public int Tag { get; set; }

        // Mutually exclusive
        public int? Qty { get; set; }
        public bool Confirm { get; set; }

        // Optional (future)
        public string? Text { get; set; }

        public bool ClearHeader { get; set; }

        public bool CheckGateaway { get; set; }
    }
}
