using MongoDB.Bson;
using MongoDB.Driver;
using PartsOrderTracker.Api.Dtos;
using PartsOrderTracker.Api.Models;

namespace PartsOrderTracker.Api.Services;

public sealed class OrdersService(
  IMongoDatabase database,
  PartsService partsService,
  ICurrentUserService currentUser,
  TimeProvider timeProvider)
{
  private readonly IMongoCollection<Order> orders =
    database.GetCollection<Order>("orders");

  public async Task<IReadOnlyList<OrderResponse>>
    GetMineAsync(CancellationToken cancellation)
  {
    var myOrders = await orders
      .Find(order => order.UserId == currentUser.UserId)
      .SortByDescending(order => order.CreatedAt)
      .ToListAsync(cancellation);

    return myOrders.ConvertAll(ToResponse);
  }

  public async Task<OrderResponse?> GetMineByIdAsync(ObjectId id, CancellationToken cancellationToken)
  {
    var document = await orders
      .Find(order => order.Id == id && order.UserId == currentUser.UserId)
      .FirstOrDefaultAsync(cancellationToken);

    return document is null ? null : ToResponse(document);
  }

  public async Task<OrderResponse?> CreateAsync(
    IReadOnlyList<(ObjectId PartId, int Quantity)> lines,
    CancellationToken cancellationToken)
  {
    var items = new List<OrderItem>();
    foreach (var (partId, quantity) in lines)
    {
      var part = await partsService.GetActiveByIdAsync(partId, cancellationToken);
      if (part is null)
      {
        return null;
      }

      items.Add(new OrderItem
      {
        PartId = partId,
        ArticleNumber = part.ArticleNumber,
        Name = part.Name,
        Manufacturer = part.Manufacturer,
        Quantity = quantity
      });
    }

    var now = timeProvider.GetUtcNow().UtcDateTime;
    var order = new Order
    {
      Id = ObjectId.GenerateNewId(),
      UserId = currentUser.UserId,
      Items = items,
      Status = OrderStatus.Received,
      CreatedAt = now,
      UpdatedAt = now,
      NextStatusChangeAt = now.AddSeconds(10),
      UserName = currentUser.UserName,
      UserEmail = currentUser.Email,
      ReceivedAt = now
    };

    await orders.InsertOneAsync(order, cancellationToken: cancellationToken);
    return ToResponse(order);
  }

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
