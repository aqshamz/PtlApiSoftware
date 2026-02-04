using Dapper;
using MySqlConnector;
using Ptl.Contracts.Dtos.Hardware;

public class PtlHardwareRepository
{
    private readonly string _cs;

    public PtlHardwareRepository(IConfiguration cfg)
    {
        _cs = cfg.GetConnectionString("PtlDb")!;
    }

    public IEnumerable<PtlGatewayConfig> GetGateways()
    {
        using var conn = new MySqlConnection(_cs);

        return conn.Query<PtlGatewayConfig>(
            """
            SELECT
                gateway_id AS GatewayId,
                port       AS Port,
                ip_address AS IpAddress
            FROM ptl_hardware
            ORDER BY gateway_id;
            """
        );
    }
}
