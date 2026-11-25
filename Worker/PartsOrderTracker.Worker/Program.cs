using Microsoft.Extensions.Options;
using MongoDB.Driver;
using PartsOrderTracker.Worker;
using PartsOrderTracker.Worker.Options;
using PartsOrderTracker.Worker.Repositories;
using PartsOrderTracker.Worker.Services;

var builder = Host.CreateApplicationBuilder(args);

//Configuration: bind and validate the "MongoDb" and "Worker" sections at startup
builder.Services.AddOptions<MongoDbOptions>()
  .Bind(builder.Configuration.GetSection("MongoDb"))
  .Validate(o => !string.IsNullOrWhiteSpace(o.ConnectionString),
    "MongoDb:ConnectionString is missing. Configure it with .NET User Secrets.")
  .Validate(o => !string.IsNullOrWhiteSpace(o.DatabaseName),
    "MongoDb:DatabaseName is missing. It must match the API's database.")
  .ValidateOnStart();

builder.Services.AddOptions<WorkerOptions>()
  .Bind(builder.Configuration.GetSection("Worker"))
  .Validate(o => o.PollIntervalSeconds is >= 1 and <= 3600, "Invalid poll interval.")
  .Validate(o => o.ProcessingDelaySeconds is >= 1 and <= 86400, "Invalid processing delay.")
  .Validate(o => o.ErrorDelaySeconds is >= 1 and <= 3600, "Invalid error delay.")
  .Validate(o => o.BatchSize is >= 1 and <= 1000, "BatchSize must be 1..1000.")
  .ValidateOnStart();

//Mongo db configuration setup
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IMongoClient>(sp =>
{
  var options = sp.GetRequiredService<IOptions<MongoDbOptions>>().Value;
  var settings = MongoClientSettings.FromConnectionString(options.ConnectionString);
  settings.ServerSelectionTimeout = TimeSpan.FromSeconds(10);
  settings.ConnectTimeout = TimeSpan.FromSeconds(10);
  return new MongoClient(settings);
});
builder.Services.AddSingleton(sp =>
{
  var options = sp.GetRequiredService<IOptions<MongoDbOptions>>().Value;
  return sp.GetRequiredService<IMongoClient>().GetDatabase(options.DatabaseName);
});

//Order processing: repository, notifier, processor and the polling loop
builder.Services.AddSingleton<IOrderRepository, MongoOrderRepository>();

// SIGNALR STEP (Worker 1/1): delete the LoggingOrderNotifier line and uncomment the SignalROrderNotifier line,
// so status changes are pushed to the UI through Azure SignalR
// (Azure:SignalR:ConnectionString is set with user-secrets).
builder.Services.AddSingleton<IOrderNotifier, LoggingOrderNotifier>();
//builder.Services.AddSingleton<IOrderNotifier, SignalROrderNotifier>();

builder.Services.AddScoped<OrderProcessor>();
builder.Services.AddHostedService<Worker>();

await builder.Build().RunAsync();
