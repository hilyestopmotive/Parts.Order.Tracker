using PartsOrderTracker.Worker.Models;

namespace PartsOrderTracker.Worker.Repositories;

public interface IOrderRepository
{
  Task<IReadOnlyList<Order>> GetDueAsync(DateTime now, CancellationToken cancellationToken);

  Task<Order?> TryAdvanceAsync(
    Order expected,
    OrderStatus nextStatus,
    DateTime now,
    DateTime? nextStatusChangeAt,
    CancellationToken cancellationToken);
}
