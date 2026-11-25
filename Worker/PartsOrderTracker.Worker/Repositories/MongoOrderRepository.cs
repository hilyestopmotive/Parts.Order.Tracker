using Microsoft.Extensions.Options;
using MongoDB.Driver;
using PartsOrderTracker.Worker.Models;
using PartsOrderTracker.Worker.Options;

namespace PartsOrderTracker.Worker.Repositories;

public sealed class MongoOrderRepository(IMongoDatabase database, IOptions<WorkerOptions> options)
  : IOrderRepository
{
  private readonly IMongoCollection<Order> orders = database.GetCollection<Order>("orders");

  public async Task<IReadOnlyList<Order>> GetDueAsync(DateTime now, CancellationToken cancellationToken)
  {
    var f = Builders<Order>.Filter;
    var filter = f.In(o => o.Status, [OrderStatus.Received, OrderStatus.Processing])
      & f.Ne(o => o.NextStatusChangeAt, null)
      & f.Lte(o => o.NextStatusChangeAt, (DateTime?)now);

    return await orders.Find(filter)
      .SortBy(o => o.NextStatusChangeAt)
      .Limit(options.Value.BatchSize)
      .ToListAsync(cancellationToken);
  }

  public async Task<Order?> TryAdvanceAsync(
    Order expected,
    OrderStatus nextStatus,
    DateTime now,
    DateTime? nextStatusChangeAt,
    CancellationToken cancellationToken)
  {
    var f = Builders<Order>.Filter;

    // Compare-and-set: the update only applies if the order is still in the state we read,
    // so the same transition can never be applied twice.
    var filter = f.Eq(o => o.Id, expected.Id)
      & f.Eq(o => o.Status, expected.Status)
      & f.Eq(o => o.UpdatedAt, expected.UpdatedAt)
      & f.Eq(o => o.NextStatusChangeAt, expected.NextStatusChangeAt)
      & f.Ne(o => o.NextStatusChangeAt, null)
      & f.Lte(o => o.NextStatusChangeAt, (DateTime?)now);

    // Partial update only: the items, user info and other fields written by the API are preserved.
    var update = Builders<Order>.Update
      .Set(o => o.Status, nextStatus)
      .Set(o => o.UpdatedAt, now)
      .Set(o => o.NextStatusChangeAt, nextStatusChangeAt);

    update = nextStatus == OrderStatus.Processing
      ? update.Set(o => o.ProcessingAt, now)
      : update.Set(o => o.DispatchedAt, now);

    return await orders.FindOneAndUpdateAsync(
      filter,
      update,
      new FindOneAndUpdateOptions<Order> { ReturnDocument = ReturnDocument.After, IsUpsert = false },
      cancellationToken);
  }
}
