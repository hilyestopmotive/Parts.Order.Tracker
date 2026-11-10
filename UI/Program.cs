using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using UI;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddHttpClient("PartsApi", client =>
{
  client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!);
})
.AddHttpMessageHandler(sp => sp.GetRequiredService<AuthorizationMessageHandler>()
  .ConfigureHandler(
    authorizedUrls: [builder.Configuration["ApiBaseUrl"]!],
    scopes: [builder.Configuration["ApiScope"]!]));

builder.Services.AddScoped<UI.Services.CartService>();
builder.Services.AddScoped<UI.Services.FavoritesService>();
builder.Services.AddScoped<UI.Services.OrderNotificationService>();

builder.Services.AddMsalAuthentication(options =>
{
  builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);

  options.ProviderOptions.LoginMode = "redirect";
});

await builder.Build().RunAsync();
