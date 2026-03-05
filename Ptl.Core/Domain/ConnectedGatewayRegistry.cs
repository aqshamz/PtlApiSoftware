public class ConnectedGatewayRegistry
{
    private readonly Dictionary<int, GatewayRuntimeInfo> _gateways = new();

    public void SetConnected(int gatewayId, string ip, string tabelAwal)
    {
        _gateways[gatewayId] = new GatewayRuntimeInfo
        {
            GatewayId = gatewayId,
            IpAddress = ip,
            TabelAwal = tabelAwal
        };
    }

    public void SetDisconnected(int gatewayId)
    {
        _gateways.Remove(gatewayId);
    }

    public IEnumerable<GatewayRuntimeInfo> GetConnected()
        => _gateways.Values;
}

public class GatewayRuntimeInfo
{
    public int GatewayId { get; set; }
    public string IpAddress { get; set; } = "";
    public string TabelAwal { get; set; } = "";
}