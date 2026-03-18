using System.Collections.Concurrent;

public class ConnectedGatewayRegistry
{
    private readonly ConcurrentDictionary<int, GatewayRuntimeInfo> _gateways = new();
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
        _gateways.TryRemove(gatewayId, out _);
    }

    public bool IsConnected(int gatewayId)
    {
        return _gateways.ContainsKey(gatewayId);
    }

    public IEnumerable<GatewayRuntimeInfo> GetConnected()
        => _gateways.Values;
}

//public class GatewayRuntimeInfo
//{
//    public int GatewayId { get; set; }
//    public string IpAddress { get; set; } = "";
//    public string TabelAwal { get; set; } = "";
//}