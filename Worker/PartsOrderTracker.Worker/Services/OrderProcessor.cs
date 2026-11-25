using Microsoft.Extensions.Options;
using PartsOrderTracker.Worker.Models;
using PartsOrderTracker.Worker.Options;
using PartsOrderTracker.Worker.Repositories;

namespace PartsOrderTracker.Worker.Services;

public sealed class OrderProcessor(
  IOrderRepository store,
  IOrderNotifier notifier,
  TimeProvider clock,
  IOptions<WorkerOptions> options,
  ILogger<OrderProcessor> logger)
{
  public async Task<int> ProcessDueAsync(CancellationToken cancellationToken)
  {
    // Fixed batch snapshot: each order advances at most once per cycle.
    var orders = await store.GetDueAsync(clock.GetUtcNow().UtcDateTime, cancellationToken);
    var changed = 0;

    foreach (var order in orders)
    {
      cancellationToken.ThrowIfCancellationRequested();

      var nextStatus = order.Status switch
      {
        OrderStatus.Received => OrderStatus.Processing,
        OrderStatus.Processing => OrderStatus.Dispatched,
        _ => throw new InvalidOperationException("Unexpected order status in due batch.")
      };

      var now = clock.GetUtcNow().UtcDateTime;
      DateTime? nextChange = nextStatus == OrderStatus.Processing
        ? now.AddSeconds(options.Value.ProcessingDelaySeconds)
        : null;

      var updated = await store.TryAdvanceAsync(order, nextStatus, now, nextChange, cancellationToken);
      if (updated is null)
      {
        continue;
      }

      changed++;
      logger.LogInformation(
        "OrderId={OrderId}: {PreviousStatus} -> {Status}", updated.Id, order.Status, updated.Status);

      try
      {
        await notifier.NotifyAsync(updated, cancellationToken);
      }
      catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
      {
        throw;
      }
      catch (Exception ex)
      {
        // The new status is already saved; a failed notification must not undo or repeat it.
        logger.LogWarning(
          "OrderId={OrderId} saved but notification failed ({ErrorType}).", updated.Id, ex.GetType().Name);
      }
    }

    return changed;
  }
}
