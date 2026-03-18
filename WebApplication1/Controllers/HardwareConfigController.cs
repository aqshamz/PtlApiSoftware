using Microsoft.AspNetCore.Mvc;
using Ptl.Contracts.Dtos.Hardware;
using System.Text.Json;

[ApiController]
[Route("ptl/hardware")]
public class HardwareConfigController : ControllerBase
{
    private readonly PtlPostgresHardwareRepository _repo; // pgsql
    private readonly ConnectedGatewayRegistry _registry; //register available ip pg
    private readonly RecoveryService _recovery;  //recovery

    public HardwareConfigController(
        PtlPostgresHardwareRepository repo, 
        ConnectedGatewayRegistry registry,
        RecoveryService recovery) // pgsql
    {
        _repo = repo;
        _registry = registry;
        _recovery = recovery;
    }

    [HttpGet("gateways")]
    public IActionResult GetGateways()
    {
        try
        {
            var gateways = _repo.GetGateways().ToList(); // get data from pg db

            System.IO.File.WriteAllText(
                "gateways_cache_api.json",
                System.Text.Json.JsonSerializer.Serialize(gateways, new JsonSerializerOptions
                {
                    WriteIndented = true
                })
            ); //put into json

            return Ok(gateways);
        }
        catch (Exception ex)
        {
            PtlLog.Warn("DB unavailable, loading cached gateways");

            if (!System.IO.File.Exists("gateways_cache_api.json")) //current json empty
                return StatusCode(500, "Gateway configuration unavailable");

            var json = System.IO.File.ReadAllText("gateways_cache_api.json"); //load old json

            var gateways = System.Text.Json.JsonSerializer
                .Deserialize<List<PtlGatewayConfigExtended>>(json); //serialize json

            return Ok(gateways);
        }
    }

    [HttpGet("status")]
    public IActionResult GetStatuses()
    {
        return Ok(_registry.GetConnected());
    }

    [HttpPost("status")]
    public async Task<IActionResult> UpdateStatus([FromBody] UpdateGatewayStatusRequest request)
    {
        GatewayRuntimeInfo? gateway = null;

        try
        {
            var gw = _repo.GetGateways().FirstOrDefault(g => g.IpAddress == request.IpAddress); //ambil dari db

            if (gw != null)
            {
                gateway = new GatewayRuntimeInfo
                {
                    GatewayId = gw.GatewayId,
                    IpAddress = gw.IpAddress,
                    TabelAwal = gw.TabelAwal
                };
            }
        }
        catch
        {
            PtlLog.Warn("DB unavailable while resolving gateaway");
        }

        if (gateway == null)
        {
            try
            {
                if (System.IO.File.Exists("gateways_cache_api.json")) //dari json
                {
                    var json = System.IO.File.ReadAllText("gateways_cache_api.json");

                    var cached = System.Text.Json.JsonSerializer
                        .Deserialize<List<PtlGatewayConfigExtended>>(json);

                    var cachedGateway = cached?
                        .FirstOrDefault(g => g.IpAddress == request.IpAddress);

                    if (cachedGateway != null)
                    {
                        gateway = new GatewayRuntimeInfo
                        {
                            GatewayId = cachedGateway.GatewayId,
                            IpAddress = cachedGateway.IpAddress,
                            TabelAwal = cachedGateway.TabelAwal
                        };
                    }
                }
            }
            catch
            {
                PtlLog.Error("Failed to load gateaway from cache");
            }
        }

        if (gateway == null)
        {

            PtlLog.Error($"Gateway config not found for IP {request.IpAddress}");

            return Ok();
        }

        if (request.Status == 1 && !_registry.IsConnected(gateway.GatewayId)) //kalo status berubah jadi 1 dari 0
        {
            _registry.SetConnected(
                gateway.GatewayId,
                gateway.IpAddress,
                gateway.TabelAwal
            );

            await Task.Delay(1500);

            try
            {
                await _recovery.RecoverGateway(gateway);
            }
            catch (Exception ex)
            {
                PtlLog.Error($"Recovery transaction gateaway {gateway.GatewayId} skipped: {ex.Message}");
            }

        }
        else if(request.Status == 0)
        {
            _registry.SetDisconnected(gateway.GatewayId); //kalo status berubah jadi 0
        }

        try
        {
            _repo.UpdateStatus(request.IpAddress, request.Status); //update db
        }
        catch
        {
            PtlLog.Error("DB unavailable, skipping status update");
        }

        return Ok();
    }
}
