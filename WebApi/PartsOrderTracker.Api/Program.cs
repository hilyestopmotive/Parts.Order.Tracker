using System.Text.Json.Serialization;
using MongoDB.Driver;
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

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
