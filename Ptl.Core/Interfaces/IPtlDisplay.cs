public interface IPtlDisplay
{
    Task ShowHeader(int gateway, int tag, string text);
    Task DisplayQty(int gateway, int tag, int qty);
    Task ClearHeader(int gateway, int tag);

    Task<IReadOnlySet<int>> GetReadyTags(int gateway);
}
