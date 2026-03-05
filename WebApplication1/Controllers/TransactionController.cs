using Microsoft.AspNetCore.Mvc;
using Ptl.Contracts.Dtos;
using Ptl.Core.Interfaces;

[ApiController]
[Route("ptl/transaction")]
public class TransactionController : ControllerBase
{
    private readonly ITransactionCommandRepository _repo;

    public TransactionController(ITransactionCommandRepository repo)
    {
        _repo = repo;
    }

    [HttpPost]
    public IActionResult Create([FromBody] PickTransactionDto dto)
    {
        _repo.InsertTransaction(dto);
        return Ok(new { dto.TxId });
    }

    [HttpPost("load-batch")]
    public async Task<IActionResult> LoadBatch(
    [FromServices] BatchLoaderService loader)
    {
        await loader.LoadTransactionsAsync();
        return Ok();
    }

}
