using System.ComponentModel.DataAnnotations;

namespace PartsOrderTracker.Api.Dtos;

public sealed class CreateOrderRequest
{
  [Required]
  [MinLength(1)]
  public List<CreateOrderItemRequest> Items { get; init; } = [];
}

public sealed class CreateOrderItemRequest
{
  [Required]
  public string PartId { get; init; } = string.Empty;

  [Range(1, 100)]
  public int Quantity { get; init; }
}