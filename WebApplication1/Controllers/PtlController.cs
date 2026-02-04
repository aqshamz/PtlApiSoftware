using Microsoft.AspNetCore.Mvc;
using Ptl.Contracts.Dtos.Hardware;
using Ptl.Core.Application;

namespace Ptl.Api.Controllers;

[ApiController]
[Route("ptl")]
public class PtlController : ControllerBase
{
    private readonly TagCommandHandler _handler;

    public PtlController(TagCommandHandler handler)
    {
        _handler = handler;
    }

    [HttpPost("rx")]
    public IActionResult Receive(PtlRxEventDto dto)
    {
        Console.WriteLine(
            $"API RX gw={dto.Gateway}, tag={dto.Tag}, cmd={dto.Command}"
        );

        _handler.Handle(dto.Tag, dto.Command);

        return Ok();
    }
}
