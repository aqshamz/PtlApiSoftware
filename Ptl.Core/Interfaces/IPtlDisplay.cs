public interface IPtlDisplay
{
    void ShowHeader(int gateway, int tag, string text);
    void DisplayQty(int gateway, int tag, int qty);
    void ClearHeader(int gateway, int tag);
}
