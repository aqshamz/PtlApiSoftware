using Microsoft.AspNetCore.Mvc;
using Ptl.Contracts.Dtos.Hardware;
using Ptl.Core.Application;

namespace Ptl.Api.Controllers;

[ApiController]
[Route("ptl")]
public class PtlController : ControllerBase
{
    //private readonly TagCommandHandler _handler; //command tag

    //public PtlController(TagCommandHandler handler) //command tag
    //{
    //    _handler = handler;
    //}

    //[HttpPost("rx")] //mysql
    //public IActionResult Receive(PtlRxEventDto dto)
    //{
    //    Console.WriteLine(
    //        $"API RX gw={dto.Gateway}, tag={dto.Tag}, cmd={dto.Command}"
    //    );

    //    _handler.Handle(dto.Tag, dto.Command);

    //    return Ok();
    //}
    [HttpPost("rx")] //pg
    public async Task<IActionResult> Receive(
    [FromBody] PtlRxEventDto evt,
    [FromServices] BatchPhase1RxService phase1Rx)
    {
        await phase1Rx.HandleAsync(evt);
        return Ok();
    }
}