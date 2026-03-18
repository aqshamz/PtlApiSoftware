using Microsoft.AspNetCore.Mvc;
using Ptl.Contracts.Dtos.Hardware;

[ApiController]
[Route("database")]
public class DatabaseController : ControllerBase
{
    private readonly PostgresConnectionFactory _db; // pgsql

    public DatabaseController(
        PostgresConnectionFactory db) // pgsql
    {
        _db = db;
    }

    [HttpGet("db-status")]
    public async Task<IActionResult> GetDbStatus()
    {
        try
        {
            var ok = await _db.TestConnectionAsync();
            return Ok(ok);
        }
        catch
        {
            return Ok(false);
        }
    }
}