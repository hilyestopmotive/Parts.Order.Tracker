using Microsoft.Azure.SignalR.Management;
using PartsOrderTracker.Worker.Models;

namespace PartsOrderTracker.Worker.Services;

// Publishes order status changes to the Azure SignalR Service. The UI receives them through the
// API's hub, in a group named after the user id. Hub and message names must match the API and UI.
public sealed class SignalROrderNotifier : IOrderNotifier, IAsyncDisposable
{
  // Must equal the Hub class name in the API (Azure SignalR uses the type name as the hub name).
  public const string HubName = "OrdersHub";

  public const string MessageName = "OrderStatusChanged";

  private readonly Task<ServiceHubContext> hubContextTask;

  public SignalROrderNotifier(IConfiguration configuration)
  {
    var connectionString = configuration["Azure:SignalR:ConnectionString"];
    if (string.IsNullOrWhiteSpace(connectionString))
    {
      throw new InvalidOperationException(
        "Azure:SignalR:ConnectionString is missing. Configure it with .NET User Secrets.");
    }

    // Build the hub context once and reuse it for every notification.
    hubContextTask = new ServiceManagerBuilder()
      .WithOptions(options => options.ConnectionString = connectionString)
      .BuildServiceManager()
      .CreateHubContextAsync(HubName, CancellationToken.None);
  }

  public async Task NotifyAsync(Order order, CancellationToken cancellationToken)
  {
    var hubContext = await hubContextTask;
    await hubContext.Clients
      .Group(order.UserId)
      .SendCoreAsync(
        MessageName,
        [new { orderId = order.Id.ToString(), status = order.Status.ToString() }],
        cancellationToken);
  }

  public async ValueTask DisposeAsync()
  {
    if (hubContextTask.IsCompletedSuccessfully)
    {
      await hubContextTask.Result.DisposeAsync();
    }
  }
}
