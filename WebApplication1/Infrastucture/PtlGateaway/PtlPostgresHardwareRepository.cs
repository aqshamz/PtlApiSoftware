using Dapper;
using Npgsql;
using Ptl.Contracts.Dtos.Hardware;

public class PtlPostgresHardwareRepository
{
    private readonly string _cs;

    public PtlPostgresHardwareRepository(IConfiguration cfg)
    {
        _cs = cfg.GetConnectionString("PgDb")!;
    }

    public IEnumerable<PtlGatewayConfigExtended> GetGateways()
    {
        using var conn = new NpgsqlConnection(_cs);

        return conn.Query<PtlGatewayConfigExtended>(
            """
            select 
                (pmih.urutan_cek + 1)::int as GatewayId,
                4660                       as Port,
                pmih.ip_hub                as IpAddress,
                pmih.zona                  as Zona,
                pmgz.tabel_awal            as TabelAwal,
                pmih.status_con::int       as StatusCon
            from ptl_master_ip_hub pmih
            join ptl_master_group_zona pmgz 
                on pmih.zona = pmgz.zona
            where pmih.group_zona = 2
            order by pmih.urutan_cek;
            """
        );
    }

    public void UpdateStatus(string ipAddress, int status)
    {
        using var conn = new NpgsqlConnection(_cs);

        conn.Execute(
            """
            update ptl_master_ip_hub
            set status_con = @Status
            where ip_hub = @IpAddress;
            """,
            new { Status = status, IpAddress = ipAddress }
        );
    }
}
