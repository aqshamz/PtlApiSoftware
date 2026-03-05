namespace Ptl.Contracts.Dtos.Hardware;

public record PtlGatewayConfigExtended(
    int GatewayId,
    int Port,
    string IpAddress,
    string Zona,
    string TabelAwal,
    int StatusCon
);
