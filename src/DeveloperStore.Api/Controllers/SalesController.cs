using DeveloperStore.Application.Sales.Commands.CancelSale;
using DeveloperStore.Application.Sales.Commands.CancelSaleItem;
using DeveloperStore.Application.Sales.Commands.CreateSale;
using DeveloperStore.Application.Sales.Commands.DeleteSale;
using DeveloperStore.Application.Sales.Commands.UpdateSale;
using DeveloperStore.Application.Sales.Queries.GetAllSales;
using DeveloperStore.Application.Sales.Queries.GetSaleById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DeveloperStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalesController : ControllerBase
{
    private readonly IMediator _mediator;

    public SalesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var sales = await _mediator.Send(new GetAllSalesQuery(), cancellationToken);
        return Ok(sales);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var sale = await _mediator.Send(new GetSaleByIdQuery(id), cancellationToken);
        return sale is null ? NotFound() : Ok(sale);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateSaleCommand command,
        CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateSaleCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("Route id does not match body id.");

        var result = await _mediator.Send(command, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _mediator.Send(new DeleteSaleCommand(id), cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var cancelled = await _mediator.Send(new CancelSaleCommand(id), cancellationToken);
        return cancelled ? NoContent() : NotFound();
    }

    [HttpPatch("{id:guid}/items/{itemId:guid}/cancel")]
    public async Task<IActionResult> CancelItem(Guid id, Guid itemId, CancellationToken cancellationToken)
    {
        var cancelled = await _mediator.Send(new CancelSaleItemCommand(id, itemId), cancellationToken);
        return cancelled ? NoContent() : NotFound();
    }
}
