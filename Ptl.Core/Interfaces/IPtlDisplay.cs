//public interface IPtlDisplay
//{
//    void ShowHeader(int gateway, int tag, string text);
//    void DisplayQty(int gateway, int tag, int qty);
//    void ClearHeader(int gateway, int tag);

//    IReadOnlySet<int> GetReadyTags(int gateway);
//}
public interface IPtlDisplay
{
    Task ShowHeader(int gateway, int tag, string text);
    Task DisplayQty(int gateway, int tag, int qty);
    Task ClearHeader(int gateway, int tag);

    Task<IReadOnlySet<int>> GetReadyTags(int gateway);
}
