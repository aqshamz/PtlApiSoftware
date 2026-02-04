using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("ptl/hardware")]
public class HardwareConfigController : ControllerBase
{
    private readonly PtlHardwareRepository _repo;

    public HardwareConfigController(PtlHardwareRepository repo)
    {
        _repo = repo;
    }

    [HttpGet("gateways")]
    public IActionResult GetGateways()
        => Ok(_repo.GetGateways());
}
