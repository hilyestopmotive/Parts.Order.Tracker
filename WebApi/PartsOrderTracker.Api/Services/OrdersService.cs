using PartsOrderTracker.Api.Dtos;
using PartsOrderTracker.Api.Models;

namespace PartsOrderTracker.Api.Services;

public sealed class OrdersService
{
  private static OrderResponse ToResponse(Order order) => new(
  order.Id.ToString(),
  GetItems(order).Select(item => new OrderItemResponse(
    item.PartId?.ToString(),
    item.ArticleNumber,
    item.Name,
    item.Manufacturer,
    item.Quantity)).ToList(),
  order.Status,
  order.CreatedAt,
  order.UpdatedAt,
  order.NextStatusChangeAt,
  order.UserName,
  order.UserEmail,
  order.ReceivedAt,
  order.ProcessingAt,
  order.DispatchedAt,
  order.ExpectedArrival,
  order.TrackingId);

  private static List<OrderItem> GetItems(Order order)
  {
    if (order.Items.Count > 0 || order.LegacyPart is null)
    {
      return order.Items;
    }

    order.LegacyPart.Quantity = order.LegacyQuantity ?? 1;
    return [order.LegacyPart];
  }
}
