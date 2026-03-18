using DAPCAPS;
using Ptl.Contracts.Dtos.Hardware;

public static class PtlInitializer
{
    private static bool _initialized;

    public static event Action<int, int>? OnGatewayStatusChanged; //pg gateaway


    public static void Init(IEnumerable<PtlGatewayConfig> gateways)
    {
        if (_initialized) return;

        Directory.SetCurrentDirectory(AppContext.BaseDirectory);

        PtlLog.Hw("Initialize Hardware");

        if (CapsAPI.AB_API_Open() <= 0)
            throw new Exception("AB_API_Open failed");

        Util.AB_LoadConf();

        foreach (var gw in gateways)
        {
            PtlLog.Hw($"Opening Gateaway {gw.GatewayId}");

            CapsAPI.AB_GW_Open(gw.GatewayId);

            //buat dari pg
            Thread.Sleep(300);

            int diag = CapsAPI.AB_GW_TagDiag(gw.GatewayId, 0);

            PtlLog.Hw($"Gateaway {gw.GatewayId} TagDiag ret={diag}");

            int statusValue = diag >= 0 ? 1 : 0;

            OnGatewayStatusChanged?.Invoke(gw.GatewayId, statusValue);
        }
        
        PtlLog.Hw($"Gateaway ready count {CapsAPI.AB_GW_Cnt()}");

        PtlLog.Hw("Initialize Done");

        _initialized = true;
    }
}
