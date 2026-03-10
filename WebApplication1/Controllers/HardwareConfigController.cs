using Microsoft.AspNetCore.Mvc;
using Ptl.Contracts.Dtos.Hardware;

[ApiController]
[Route("ptl/hardware")]
public class HardwareConfigController : ControllerBase
{
    //private readonly PtlHardwareRepository _repo; // mysql
    private readonly PtlPostgresHardwareRepository _repo; // pgsql
    private readonly ConnectedGatewayRegistry _registry; //register available ip pg
    private readonly RecoveryService _recovery;  //recovery

    //public HardwareConfigController(PtlHardwareRepository repo) // mysql
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
        => Ok(_repo.GetGateways());

    [HttpPost("status")]
    public async Task<IActionResult> UpdateStatus([FromBody] UpdateGatewayStatusRequest request)
    {
        _repo.UpdateStatus(request.IpAddress, request.Status);

        var gateway = _repo.GetGateways()
            .First(g => g.IpAddress == request.IpAddress);

        if (request.Status == 1 && !_registry.IsConnected(gateway.GatewayId))
        {
            _registry.SetConnected(
                gateway.GatewayId,
                gateway.IpAddress,
                gateway.TabelAwal
            );

            await Task.Delay(1500);

            await _recovery.RecoverGateway(new GatewayRuntimeInfo
            {
                GatewayId = gateway.GatewayId,
                IpAddress = gateway.IpAddress,
                TabelAwal = gateway.TabelAwal
            });
        }
        else
        {
            _registry.SetDisconnected(gateway.GatewayId);
        }

        return Ok();
    }
}
