using Npgsql;
using Microsoft.Extensions.Configuration;

public class PostgresConnectionFactory
{
    private readonly string _connectionString;

    public PostgresConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("PgDb");
    }

    public NpgsqlConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }

    public async Task<NpgsqlConnection> CreateOpenConnectionAsync()
    {
        var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        return conn;
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();

            //Console.WriteLine("PostgreSQL Connected Successfully!");
            return true;
        }
        catch (Exception ex)
        {
            //Console.WriteLine("PostgreSQL Connection Failed:");
            Console.WriteLine(ex.Message);
            return false;
        }
    }

}
