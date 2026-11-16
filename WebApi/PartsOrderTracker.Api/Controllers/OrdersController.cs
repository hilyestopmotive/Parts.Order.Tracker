using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using PartsOrderTracker.Api.Dtos;
using PartsOrderTracker.Api.Services;

namespace PartsOrderTracker.Api.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController(OrdersService ordersService) : ControllerBase
{
  [HttpGet("my")]
  [ProducesResponseType<IReadOnlyList<OrderResponse>>(StatusCodes.Status200OK)]
  public async Task<ActionResult<IReadOnlyList<OrderResponse>>> GetMine(CancellationToken cancellationToken)
  {
    return Ok(await ordersService.GetMineAsync(cancellationToken));
  }

  [HttpGet("{id}")]
  [ProducesResponseType<OrderResponse>(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<OrderResponse>> GetById(string id, CancellationToken cancellationToken)
  {
    if (!ObjectId.TryParse(id, out var objectId))
    {
      ModelState.AddModelError(nameof(id), "The order ID must be a valid MongoDB ObjectId.");
      return ValidationProblem(ModelState);
    }

    var order = await ordersService.GetMineByIdAsync(objectId, cancellationToken);
    return order is null ? NotFound() : Ok(order);
  }

  [HttpPost]
  [ProducesResponseType<OrderResponse>(StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<ActionResult<OrderResponse>> Create(
    CreateOrderRequest request,
    CancellationToken cancellationToken)
  {
    var lines = new List<(ObjectId PartId, int Quantity)>();
    for (var i = 0; i < request.Items.Count; i++)
    {
      var item = request.Items[i];
      if (!ObjectId.TryParse(item.PartId, out var partId))
      {
        ModelState.AddModelError($"{nameof(request.Items)}[{i}].PartId", "The part ID must be a valid MongoDB ObjectId.");
        return ValidationProblem(ModelState);
      }

      lines.Add((partId, item.Quantity));
    }

    var order = await ordersService.CreateAsync(lines, cancellationToken);
    if (order is null)
    {
      ModelState.AddModelError(nameof(request.Items), "One or more parts do not exist or are inactive.");
      return ValidationProblem(ModelState);
    }

    return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
  }
}
