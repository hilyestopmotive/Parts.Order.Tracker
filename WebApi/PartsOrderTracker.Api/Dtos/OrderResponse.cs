using PartsOrderTracker.Api.Models;

namespace PartsOrderTracker.Api.Dtos;

public sealed record OrderResponse(
  string Id,
  IReadOnlyList<OrderItemResponse> Items,
  OrderStatus Status,
  DateTime CreatedAt,
  DateTime UpdatedAt,
  DateTime? NextStatusChangeAt,
  string UserName,
  string UserEmail,
  DateTime? ReceivedAt,
  DateTime? ProcessingAt,
  DateTime? DispatchedAt,
  DateTime? ExpectedArrival,
  string TrackingId);

public sealed record OrderItemResponse(
  string? PartId,
  string ArticleNumber,
  string Name,
  string Manufacturer,
  int Quantity);