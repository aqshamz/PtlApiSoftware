using DAPCAPS;
using Ptl.Contracts.Dtos.Hardware;
using Ptl.Hardware;

public static class PtlInitializer
{
    private static bool _initialized;

    public static event Action<int, int>? OnGatewayStatusChanged; //pg gateaway


    public static void Init(IEnumerable<PtlGatewayConfig> gateways)
    {
        if (_initialized) return;

        Directory.SetCurrentDirectory(AppContext.BaseDirectory);

        Console.WriteLine("CAPS init...");

        if (CapsAPI.AB_API_Open() <= 0)
            throw new Exception("AB_API_Open failed");

        Util.AB_LoadConf();

        foreach (var gw in gateways)
        {
            Console.WriteLine($"Opening GW {gw.GatewayId}");
            CapsAPI.AB_GW_Open(gw.GatewayId);

            //buat dari pg
            Thread.Sleep(300);

            int diag = CapsAPI.AB_GW_TagDiag(gw.GatewayId, 0);

            Console.WriteLine($"GW {gw.GatewayId} TagDiag ret={diag}");

            int statusValue = diag >= 0 ? 1 : 0;

            OnGatewayStatusChanged?.Invoke(gw.GatewayId, statusValue);
        }
        //Util.m_CurGwID = 0;

        //Util.Dap_Setup();

        
        Console.WriteLine($"GW COUNT = {CapsAPI.AB_GW_Cnt()}");

        Console.WriteLine("CAPS init done");

        _initialized = true;
    }
}
