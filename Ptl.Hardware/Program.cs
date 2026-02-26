using DAPCAPS;
using Ptl.Contracts.Dtos.Hardware;
using Ptl.Hardware;
using System.Net;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

internal static class Program
{
    [DllImport("kernel32.dll")]
    private static extern bool AllocConsole();

    [STAThread]
    static void Main()
    {
        AllocConsole();

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        Console.WriteLine("PTL Hardware starting (WinForms host)...");

        // 1️⃣ Fetch gateway config from API
        var api = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:62327")
        };

        Console.WriteLine("[HW] Fetching gateway config...");

        var gateways = api
            .GetFromJsonAsync<List<PtlGatewayConfig>>("/ptl/hardware/gateways")
            .GetAwaiter()
            .GetResult();

        if (gateways == null || gateways.Count == 0)
            throw new Exception("No PTL gateways returned from API");

        // 2️⃣ Write IPINDEX
        IpIndexWriter.Write(gateways);

        // 3️⃣ Init CAPS (NOW it sees IPINDEX)
        PtlInitializer.OnGatewayStatusChanged += async (gatewayId, status) =>
        {
            var gw = gateways.First(g => g.GatewayId == gatewayId);

            await api.PostAsJsonAsync(
                "/ptl/hardware/status",
                new UpdateGatewayStatusRequest(gw.IpAddress, status)
            );

            Console.WriteLine($"[HW] Status sent → ip={gw.IpAddress}, status={status}");
        };//gateaway pg

        PtlInitializer.Init(gateways);

        // 4️⃣ Start hardware display + API host
        var display = new HardwarePtlDisplay();
        HardwareApiHost.Start(display);

        // 5️⃣ RX loop
        EventLoop loop = new EventLoop();

        loop.OnRx += async evt =>
        {
            if (evt.Command == 252)
                return;

            Console.WriteLine(
                $"RX → API gw={evt.Gateway}, tag={evt.Tag}, cmd={evt.Command}"
            );

            await api.PostAsJsonAsync("/ptl/rx", evt);
        };

        loop.Start();

        // 6️⃣ Win32 pump
        Application.Run(new ApplicationContext());
    }
}
