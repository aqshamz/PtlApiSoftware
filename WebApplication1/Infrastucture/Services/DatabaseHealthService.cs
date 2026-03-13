public class DatabaseHealthService
{
    private readonly PostgresConnectionFactory _factory;

    public bool IsHealthy { get; private set; } = true;

    public DatabaseHealthService(PostgresConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<bool> CheckAsync()
    {
        try
        {
            using var conn = _factory.CreateConnection();
            await conn.OpenAsync();

            IsHealthy = true;
            return true;
        }
        catch
        {
            IsHealthy = false;
            return false;
        }
    }
}