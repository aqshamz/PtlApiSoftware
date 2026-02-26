using Microsoft.AspNetCore.Mvc;
using Ptl.Contracts.Dtos.Hardware;

[ApiController]
[Route("ptl/hardware")]
public class HardwareConfigController : ControllerBase
{
    //private readonly PtlHardwareRepository _repo; // mysql
    private readonly PtlPostgresHardwareRepository _repo; // pgsql
    private readonly ConnectedGatewayRegistry _registry; //register available ip pg

    //public HardwareConfigController(PtlHardwareRepository repo) // mysql
    public HardwareConfigController(
        PtlPostgresHardwareRepository repo, 
        ConnectedGatewayRegistry registry) // pgsql
    {
        _repo = repo;
        _registry = registry;
    }

    [HttpGet("gateways")]
    public IActionResult GetGateways()
        => Ok(_repo.GetGateways());

    [HttpPost("status")]
    public IActionResult UpdateStatus([FromBody] UpdateGatewayStatusRequest request)
    {
        _repo.UpdateStatus(request.IpAddress, request.Status);

        var gateway = _repo.GetGateways()
        .First(g => g.IpAddress == request.IpAddress);

        if (request.Status == 1)
        {
            _registry.SetConnected(
                gateway.GatewayId,
                gateway.IpAddress,
                gateway.TabelAwal
            );
        }
        else
        {
            _registry.SetDisconnected(gateway.GatewayId);
        }


        return Ok();
    }
}
