namespace Ptl.Contracts.Dtos.Hardware
{
    public class PtlRxEventDto
    {
        public int Gateway { get; set; }
        public int Tag { get; set; }
        public short Command { get; set; }
        public short? Qty { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }
}
