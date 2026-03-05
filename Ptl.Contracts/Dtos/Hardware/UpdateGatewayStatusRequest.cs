namespace Ptl.Contracts.Dtos.Hardware;

public record UpdateGatewayStatusRequest(
    string IpAddress,
    int Status
);