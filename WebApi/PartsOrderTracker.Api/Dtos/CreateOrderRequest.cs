namespace PartsOrderTracker.Api.Dtos;

public sealed class CreateOrderRequest
{
  public List<CreateOrderItemRequest> Items { get; init; } = [];
}

public sealed class CreateOrderItemRequest
{
  public string PartId { get; init; } = string.Empty;

  public int Quantity { get; init; }
}
