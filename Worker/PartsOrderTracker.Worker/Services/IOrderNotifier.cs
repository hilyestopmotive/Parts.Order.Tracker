using PartsOrderTracker.Worker.Models;

namespace PartsOrderTracker.Worker.Services;

public interface IOrderNotifier
{
  Task NotifyAsync(Order order, CancellationToken cancellationToken);
}
