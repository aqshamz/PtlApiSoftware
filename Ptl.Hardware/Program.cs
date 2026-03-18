using DAPCAPS;
using Microsoft.Extensions.Logging;
using Ptl.Contracts.Dtos.Hardware;
using Ptl.Hardware;
using System.Net;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Text.Json;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        PtlLog.Hw("PTL Hardware Starting (WinForms host)...");

        // 1️⃣ Fetch gateway config from API
        var api = new HttpClient
        {
            //BaseAddress = new Uri("http://127.0.0.1:5000")
            BaseAddress = new Uri("http://127.0.0.1:5000/")
        };

        PtlLog.Hw("Fetching gateway config...");
        List<PtlGatewayConfig>? gateways = null;

        try
        {
            //var res = api.GetAsync("/ptl/hardware/gateways").GetAwaiter().GetResult(); //get from api
            var res = api.GetAsync("ptl/hardware/gateways").Result;

            if (res.IsSuccessStatusCode)
            {
                gateways = res.Content
                    .ReadFromJsonAsync<List<PtlGatewayConfig>>()
                    .GetAwaiter()
                    .GetResult(); //result api

                if (gateways != null && gateways.Count > 0)
                {
                    File.WriteAllText(
                        "gateways_cache.json",
                        System.Text.Json.JsonSerializer.Serialize(gateways, new JsonSerializerOptions
                        {
                            WriteIndented = true
                        }) //get from json
                    );
                    PtlLog.Hw("Gateway config loaded from API");
                }
            }
            else
            {
                PtlLog.Hw($"API returned {res.StatusCode}");
            }
        }
        catch
        {
            PtlLog.Hw("API unreachable, loading cached gateway config");
        }

        if (gateways == null || gateways.Count == 0) // API failed
        {
            if (!File.Exists("gateways_cache.json"))
                throw new Exception("No gateway config available (API and cache failed)"); //json ilang

            var json = File.ReadAllText("gateways_cache.json");

            if (string.IsNullOrWhiteSpace(json))
                throw new Exception("Gateway cache file exists but is empty"); //json kosong

            gateways = System.Text.Json.JsonSerializer
                .Deserialize<List<PtlGatewayConfig>>(json);

            if (gateways == null || gateways.Count == 0)
                throw new Exception("Gateway cache file contains no gateway configuration"); //json gajelas


            PtlLog.Hw("Gateway config loaded from local cache");
        }

        // 2️⃣ Write IPINDEX
        PtlLog.Hw("Writing IPINDEX");
        IpIndexWriter.Write(gateways); //create ipindex

        //// 3️⃣ Init CAPS (NOW it sees IPINDEX)
        PtlInitializer.Init(gateways); //init ptl

        // 4️⃣ Start hardware display + API host
        var display = new HardwarePtlDisplay(); //connect api dan hardware
        HardwareApiHost.Start(display);

        var store = new HardwareEventStore(); //function json
        PtlLog.Hw("Recovery Worker Start");
        var retryWorker = new HardwareEventRetryWorker(api, store); //retry function json
        retryWorker.Start(); //function start looping

        // 5️⃣ RX loop
        PtlLog.Hw("Hardware Ready");
        EventLoop loop = new EventLoop(gateways); //loop receive, ada juga buat cek status gateaway 5 detik sekali

        loop.OnGatewayStatusChanged += async (gatewayId, status) => //kalo status gateaway berubah
        {
            var gw = gateways.First(g => g.GatewayId == gatewayId);

            try
            {
                await api.PostAsJsonAsync(
                    "/ptl/hardware/status",
                    new UpdateGatewayStatusRequest(gw.IpAddress, status)
                );
                PtlLog.Hw($"Status Gateaway IP {gw.IpAddress} {status} sended to API");
            }
            catch
            {
                PtlLog.Hw("API unreachable, gateway status not sent");
            }
        };

        loop.OnRx += async evt =>
        {
            if (evt.Command == 252)
                return;

            PtlLog.Hw($"Sending Message from gateaway {evt.Gateway} tag {evt.Tag} command {evt.Command} to API");

            store.Add(evt); //store to json

            try
            {
                var res = await api.PostAsJsonAsync("/ptl/rx", evt); //send to api message

                if (res.IsSuccessStatusCode)
                {
                    store.Remove(evt);
                }
                else
                {
                    PtlLog.Hw($"API error {res.StatusCode}, event queued");
                }
            }
            catch
            {
                PtlLog.Hw("API unavailable, event stored");
            }

        };

        loop.Start();

        // 6️⃣ Win32 pump
        Application.Run(new ApplicationContext());
    }
}
