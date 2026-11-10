using PartsOrderTracker.Api.Models;

namespace PartsOrderTracker.Api.Dtos;

public sealed record OrderResponse(
  string Id,
  IReadOnlyList<OrderItemResponse> Items,
  OrderStatus Status,
  DateTime CreatedAt);

public sealed record OrderItemResponse(
  string PartId,
  string ArticleNumber,
  string Name,
  int Quantity);
