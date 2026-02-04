namespace Ptl.Contracts.Dtos.Hardware;

public record PtlGatewayConfig(
    int GatewayId,
    int Port,
    string IpAddress
);
