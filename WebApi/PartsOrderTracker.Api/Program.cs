using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using MongoDB.Driver;
using PartsOrderTracker.Api.Hubs;
using PartsOrderTracker.Api.Services;

var builder = WebApplication.CreateBuilder(args);

//Enable Controller based web API and configure JSON serialization options
builder.Services
  .AddControllers()
  .AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();

//CORS: allow the Blazor UI (different origin) to call this API; AllowCredentials is required by SignalR
builder.Services.AddCors(options =>
  options.AddPolicy("UiPolicy", policy =>
    policy.WithOrigins("https://localhost:7053")
      .AllowAnyHeader()
      .AllowAnyMethod()
      .AllowCredentials()));

//Authentication: validate the Entra ID access token sent by the UI ("AzureAd" section, IDs in user-secrets)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
  .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
builder.Services.AddAuthorization();

//SignalR through Azure SignalR Service (Azure:SignalR:ConnectionString in user-secrets)
builder.Services.AddSignalR().AddAzureSignalR(
  builder.Configuration["Azure:SignalR:ConnectionString"]);

//Mongo db configuration setup
var connectionString = builder.Configuration["MongoDb:ConnectionString"];
if (string.IsNullOrWhiteSpace(connectionString))
{
  throw new InvalidOperationException(
    "MongoDb:ConnectionString is missing. Configure it with .NET User Secrets.");
}

var databaseName = builder.Configuration["MongoDb:DatabaseName"];
if (string.IsNullOrWhiteSpace(databaseName))
{
  throw new InvalidOperationException("MongoDb:DatabaseName is missing.");
}

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(connectionString));
builder.Services.AddSingleton(sp =>
  sp.GetRequiredService<IMongoClient>().GetDatabase(databaseName));
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, ClaimsCurrentUserService>();
builder.Services.AddScoped<PartsService>();
builder.Services.AddScoped<OrdersService>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
  app.UseSwagger();
  app.UseSwaggerUI();
}

//Order matters: UseCors -> UseAuthentication -> UseAuthorization -> endpoints
app.UseCors("UiPolicy");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

//Every API endpoint requires a logged-in user (anonymous calls get 401)
app.MapControllers().RequireAuthorization();

//The UI connects to {ApiBaseUrl}/hubs/orders for live order updates
app.MapHub<OrdersHub>("/hubs/orders");

app.Run();
