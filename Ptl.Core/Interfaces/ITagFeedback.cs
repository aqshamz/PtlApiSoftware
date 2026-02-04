public interface ITagFeedback
{
    void ConfirmTag(int gateway, int tag);
    void DisplayQty(int gateway, int tag, int qty);
}

