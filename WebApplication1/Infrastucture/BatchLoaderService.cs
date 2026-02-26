using Npgsql;
using Dapper;

public class BatchLoaderService
{
    private readonly ConnectedGatewayRegistry _registry;
    private readonly IConfiguration _cfg;

    public BatchLoaderService(
        ConnectedGatewayRegistry registry,
        IConfiguration cfg)
    {
        _registry = registry;
        _cfg = cfg;
    }

    public async Task LoadTransactionsAsync()
    {
        var cs = _cfg.GetConnectionString("PgDb");

        using var conn = new NpgsqlConnection(cs);
        await conn.OpenAsync();

        foreach (var gw in _registry.GetConnected())
        {
            var sql = $"""
                select * 
                from {gw.TabelAwal}
                where flag_batch = 4
            """;

            var rows = await conn.QueryAsync(sql);

            Console.WriteLine(
                $"Gateway {gw.GatewayId} loaded {rows.Count()} rows from {gw.TabelAwal}"
            );
        }
    }
}