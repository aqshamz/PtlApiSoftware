using DAPCAPS;
using Ptl.Contracts.Dtos.Hardware;

public class EventLoop
{
    public event Action<PtlRxEventDto>? OnRx;
    public event Action<int, int>? OnGatewayStatusChanged;

    private readonly List<PtlGatewayConfig> _gateways;

    private readonly Dictionary<int, int> _lastStatus = new();

    public EventLoop(List<PtlGatewayConfig> gateways)
    {
        _gateways = gateways;

        foreach (var g in gateways)
            _lastStatus[g.GatewayId] = -1;
    }

    public void Start()
    {
        new Thread(Loop)
        {
            IsBackground = true
        }.Start();
    }

    private void Loop()
    {
        byte[] buffer = new byte[255];
        int len;

        PtlLog.Hw("Rx Loop Received Message");

        DateTime lastGatewayCheck = DateTime.MinValue;

        while (true)
        {
            int gw = 0;
            int node = 0;
            short cmd = -1;
            short type = -1;
            len = 0;

            int ret = CapsAPI.AB_Tag_RcvMsg(
                ref gw,
                ref node,
                ref cmd,
                ref type,
                ref buffer[0],
                ref len
            );

            if (ret > 0 && cmd != 9)
            {
                int tag = Math.Abs(node);

                var evt = new PtlRxEventDto
                {
                    Gateway = gw,
                    Tag = tag,
                    Command = cmd
                };

                OnRx?.Invoke(evt);
            }

            // gateway health check every 5 sec
            if ((DateTime.Now - lastGatewayCheck).TotalSeconds > 5) //cek status gateaway tiap 5 detik
            {
                CheckGateways();
                lastGatewayCheck = DateTime.Now;
            }

            Thread.Sleep(50);
        }
    }

    private void CheckGateways()
    {
        //PtlLog.Hw("Checking Status Gateaway");
        foreach (var gw in _gateways)
        {
            int diag = CapsAPI.AB_GW_TagDiag(gw.GatewayId, 0);
            int status = diag >= 0 ? 1 : 0;

            if (_lastStatus[gw.GatewayId] != status)
            {
                _lastStatus[gw.GatewayId] = status;

                PtlLog.Hw($"Monitoring = Gateaway {gw.GatewayId} Status {status}");

                OnGatewayStatusChanged?.Invoke(gw.GatewayId, status);
            }
        }
    }
}