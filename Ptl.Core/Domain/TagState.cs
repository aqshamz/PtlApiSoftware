namespace Ptl.Agent.Domain
{
    public class TagState
    {
        public int Gateaway { get; }
        public int Tag { get; }
        public int Quantity { get; private set; }
        public string TxDetailId { get; }

        public TagState(int gateaway, int tag, int quantity, string txDetailId)
        {
            Gateaway = gateaway;
            Tag = tag;
            Quantity = quantity;
            TxDetailId = txDetailId;
        }

        public bool IsEmpty => Quantity <= 0;

        public void Decrease()
        {
            if (Quantity > 0) { 
                Quantity--;
            }
        }
    }
}
