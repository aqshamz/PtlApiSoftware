using Microsoft.AspNetCore.Mvc;
using Ptl.Contracts.Dtos.Hardware;

namespace Ptl.Api.Controllers;

[ApiController]
[Route("ptl")]
public class PtlController : ControllerBase
{
    [HttpPost("rx")] //pg
    public async Task<IActionResult> Receive(
    [FromBody] PtlRxEventDto evt,
    [FromServices] BatchRxService phase1Rx)
    {
        try
        {
            await phase1Rx.HandleAsync(evt);

            return Ok();
        }
        catch (Npgsql.NpgsqlException)
        {
            PtlLog.Error("DB unavailable while process transaction, RX rejected");
            
            return StatusCode(503, "Database unavailable");
        }
        catch (Exception ex)
        {
            PtlLog.Error($"DB unavailable while process transaction, {ex.Message}");
            return StatusCode(500);
        }

    }
}