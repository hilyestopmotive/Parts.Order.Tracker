using PartsOrderTracker.Worker.Models;

namespace PartsOrderTracker.Worker.Services;

// TEMPORARY (before SignalR): only writes the change to the log - nobody is notified live,
// so the UI shows the new status only after a page refresh.
// SIGNALR STEP (Worker 1/1): Program.cs registers SignalROrderNotifier instead; this class can then be deleted.
public sealed class LoggingOrderNotifier(ILogger<LoggingOrderNotifier> logger) : IOrderNotifier
{
  public Task NotifyAsync(Order order, CancellationToken cancellationToken)
  {
    logger.LogInformation(
      "OrderId={OrderId}: {Status} saved. No live notification yet (SignalR not connected).",
      order.Id,
      order.Status);
    return Task.CompletedTask;
  }
}
