using System.Text.Json.Serialization;
using MongoDB.Driver;
using PartsOrderTracker.Api.Services;
// LOGIN STEP (API 1/5): usings for token validation (package Microsoft.Identity.Web is already referenced)
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

//Enable Controller based web API and configure JSON serialization options
builder.Services
  .AddControllers()
  .AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();

//CORS: allow the Blazor UI (different origin) to call this API
builder.Services.AddCors(options =>
  options.AddPolicy("UiPolicy", policy =>
    policy.WithOrigins("https://localhost:7053")
      .AllowAnyHeader()
      .AllowAnyMethod()
      .AllowCredentials()));

// LOGIN STEP (API 2/5): validate the Entra ID access token that the UI sends in the Authorization header.
// The settings come from the "AzureAd" section (TenantId and ClientId are set with user-secrets).
//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//  .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
//builder.Services.AddAuthorization();

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

app.UseCors("UiPolicy");

app.UseHttpsRedirection();

// LOGIN STEP (API 3/5): read the token (who are you?) and check access (are you allowed?).
// Keep this order: UseCors -> UseAuthentication -> UseAuthorization -> MapControllers.
//app.UseAuthentication();
//app.UseAuthorization();

// LOGIN STEP (API 4/5): delete the next line and uncomment the one after it,
// so every API endpoint requires a logged-in user (anonymous calls get 401).
app.MapControllers();
//app.MapControllers().RequireAuthorization();

app.Run();
