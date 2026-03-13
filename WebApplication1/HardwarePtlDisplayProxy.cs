using Ptl.Contracts.Dtos.Hardware;
using System.Net.Http.Json;


public class HardwarePtlDisplayProxy : IPtlDisplay
{
    private readonly HttpClient _http;

    public HardwarePtlDisplayProxy(HttpClient http)
    {
        _http = http;
    }

    public Task DisplayQty(int gateway, int tag, int qty)
        => _http.PostAsJsonAsync("/ptl/execute", new PtlTxCommandDto
        {
            Gateway = gateway,
            Tag = tag,
            Qty = qty
        });

    public Task ClearHeader(int gateway, int tag)
        => _http.PostAsJsonAsync("/ptl/execute", new PtlTxCommandDto
        {
            Gateway = gateway,
            Tag = tag,
            ClearHeader = true
        });

    public Task ShowHeader(int gateway, int tag, string text)
        => _http.PostAsJsonAsync("/ptl/execute", new PtlTxCommandDto
        {
            Gateway = gateway,
            Tag = tag,
            Text = text
        });

    public async Task<IReadOnlySet<int>> GetReadyTags(int gateway)
    {
        var result = await _http.GetFromJsonAsync<HashSet<int>>(
            $"/ptl/ready-tags/{gateway}"
        );

        return result ?? new HashSet<int>();
    }
}

