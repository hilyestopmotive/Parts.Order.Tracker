namespace UI.Services;

using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR.Client;

/// <summary>
/// Keeps a live SignalR connection to the API's "orders" hub and refreshes the
/// user's orders when the worker publishes a status change.
/// </summary>
public sealed class OrderNotificationService(
  IConfiguration configuration,
  AuthenticationStateProvider authStateProvider,
  CartService cartService) : IAsyncDisposable
{
  // Must match the message name the worker sends.
  private const string MessageName = "OrderStatusChanged";

  private HubConnection? connection;

  public async Task StartAsync()
  {
    if (connection is not null)
    {
      return;
    }

    // The API stamps each order with the signed-in user's object id (oid), so we subscribe
    // to the group named after that same claim to receive this user's updates.
    var user = (await authStateProvider.GetAuthenticationStateAsync()).User;
    var userId = user.FindFirst("oid")?.Value
      ?? user.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value
      ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value
      ?? user.FindFirst("sub")?.Value;
    if (string.IsNullOrEmpty(userId))
    {
      return;
    }

    var hubUrl = $"{configuration["ApiBaseUrl"]!.TrimEnd('/')}/hubs/orders";
    connection = new HubConnectionBuilder()
      .WithUrl(hubUrl)
      .WithAutomaticReconnect()
      .Build();

    connection.On<OrderStatusNotification>(MessageName, async _ =>
    {
      await cartService.RefreshOrdersAsync();
    });

    await connection.StartAsync();
    await connection.InvokeAsync("Subscribe", userId);
  }

  public async ValueTask DisposeAsync()
  {
    if (connection is not null)
    {
      await connection.DisposeAsync();
    }
  }

  private sealed record OrderStatusNotification(string OrderId, string Status);
}
