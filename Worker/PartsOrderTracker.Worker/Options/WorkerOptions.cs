namespace PartsOrderTracker.Worker.Options;

public sealed class WorkerOptions
{
  public int PollIntervalSeconds { get; set; } = 5;

  public int ProcessingDelaySeconds { get; set; } = 15;

  public int ErrorDelaySeconds { get; set; } = 10;

  public int BatchSize { get; set; } = 100;
}
