// SIGNALR STEP (API 1/3): select everything BELOW this header (from "//using" to the end) and uncomment it (Ctrl+K, Ctrl+U).
// The hub is the endpoint the UI connects to (/hubs/orders). With Azure SignalR, the API only "negotiates":
// the browser then talks to Azure SignalR Service directly.
// The class name "OrdersHub" is the hub name - it must equal HubName in the Worker's SignalROrderNotifier.

//using Microsoft.AspNetCore.SignalR;

//namespace PartsOrderTracker.Api.Hubs;

//// The UI calls Subscribe with its user id (oid) and joins the group the Worker sends to.
//public sealed class OrdersHub : Hub
//{
//  public Task Subscribe(string userId) =>
//    Groups.AddToGroupAsync(Context.ConnectionId, userId);
//}
