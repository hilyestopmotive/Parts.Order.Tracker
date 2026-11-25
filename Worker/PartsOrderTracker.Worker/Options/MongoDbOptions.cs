namespace PartsOrderTracker.Worker.Options;

public sealed class MongoDbOptions
{
  public string ConnectionString { get; set; } = string.Empty;

  // Must be the same database the API uses.
  public string DatabaseName { get; set; } = string.Empty;
}
