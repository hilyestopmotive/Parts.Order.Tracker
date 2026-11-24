using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UI;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

var partsApiClient = builder.Services.AddHttpClient("PartsApi", client =>
{
  client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!);
});

// LOGIN STEP (UI 2/2): attach the logged-in user's access token to every call to the API.
// Only calls to ApiBaseUrl get the token, requested for the API's scope (ApiScope).
//partsApiClient.AddHttpMessageHandler(sp => sp.GetRequiredService<AuthorizationMessageHandler>()
//  .ConfigureHandler(
//    authorizedUrls: [builder.Configuration["ApiBaseUrl"]!],
//    scopes: [builder.Configuration["ApiScope"]!]));

builder.Services.AddScoped<UI.Services.CartService>();
builder.Services.AddScoped<UI.Services.FavoritesService>();
builder.Services.AddScoped<UI.Services.OrderNotificationService>();

// LOGIN STEP (UI 1/2): sign in with Microsoft Entra ID (MSAL).
// The settings come from the "AzureAd" section of wwwroot/appsettings.json, overridden locally by
// wwwroot/appsettings.Development.json (user-secrets do NOT work here: the UI runs in the browser).
//builder.Services.AddMsalAuthentication(options =>
//{
//  builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
//  options.ProviderOptions.LoginMode = "redirect";
//});

// TEMPORARY (before login): every visitor is "not logged in", so the app can start without Entra ID.
// TryAdd only registers it while AddMsalAuthentication above is still commented out.
// After the Login step these 2 lines do nothing and can be deleted (with Services/AnonymousAuthenticationStateProvider.cs).
builder.Services.AddAuthorizationCore();
builder.Services.TryAddScoped<AuthenticationStateProvider, UI.Services.AnonymousAuthenticationStateProvider>();

await builder.Build().RunAsync();
