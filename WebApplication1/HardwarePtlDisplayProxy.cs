//using Ptl.Contracts.Dtos.Hardware;
//using Ptl.Core.Interfaces;
//using System.Net.Http.Json;

//public class HardwarePtlDisplayProxy : IPtlDisplay
//{
//    private readonly HttpClient _http;

//    public HardwarePtlDisplayProxy(HttpClient http)
//    {
//        _http = http;
//    }

//    public async void DisplayQty(int gateway, int tag, int qty)
//    {
//        try { 
//            await _http.PostAsJsonAsync("/ptl/execute", new PtlTxCommandDto
//            {
//                Gateway = gateway,
//                Tag = tag,
//                Qty = qty
//            });
//        } 
//        catch(HttpRequestException) {
//            Console.WriteLine("[HW] Hardware not reachable");
//        }
//    }

//    public async void ClearHeader(int gateway, int tag)
//    {
//        try {
//            await _http.PostAsJsonAsync("/ptl/execute", new PtlTxCommandDto
//            {
//                Gateway = gateway,
//                Tag = tag,
//                ClearHeader = true
//            });
//        }
//        catch (HttpRequestException) {
//            Console.WriteLine("[HW] Hardware not reachable");
//        }
//    }

//    public async void ShowHeader(int gateway, int tag, string text)
//    {
//        try {
//            await _http.PostAsJsonAsync("/ptl/execute", new PtlTxCommandDto
//            {
//                Gateway = gateway,
//                Tag = tag,
//                Text = text
//            });
//        }
//        catch (HttpRequestException) {
//            Console.WriteLine("[HW] Hardware not reachable");
//        }
//    }

//    public async void GetReadyTags(int gateway)
//    {
//        try
//        {
//            await _http.PostAsJsonAsync("/ptl/execute", new PtlTxCommandDto
//            {
//                CheckGateaway = true
//            });
//        }
//        catch (HttpRequestException)
//        {
//            Console.WriteLine("[HW] Hardware not reachable");
//        }
//    }
//}

using Ptl.Contracts.Dtos.Hardware;
using Ptl.Core.Interfaces;
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

