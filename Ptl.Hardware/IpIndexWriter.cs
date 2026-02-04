using Ptl.Contracts.Dtos.Hardware;
using System.Text;

public static class IpIndexWriter
{
    private const string FILE_NAME = "IPINDEX";
    
    public static void Write(IEnumerable<PtlGatewayConfig> gateways)
    {
        var sb = new StringBuilder();

        foreach (var gw in gateways)
        {
            sb.AppendLine($"{gw.GatewayId} {gw.Port} {gw.IpAddress}");
        }

        File.WriteAllText(
            Path.Combine(AppContext.BaseDirectory, FILE_NAME),
            sb.ToString(),
            Encoding.ASCII
        );

        Console.WriteLine("IPINDEX written:");
        Console.WriteLine(sb.ToString());
    }
}

