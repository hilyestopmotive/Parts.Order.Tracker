using Microsoft.Extensions.Options;
using MongoDB.Driver;
using PartsOrderTracker.Worker.Options;
using PartsOrderTracker.Worker.Services;

namespace PartsOrderTracker.Worker;

public sealed class Worker(
  IServiceScopeFactory scopeFactory,
  IOptions<WorkerOptions> options,
  ILogger<Worker> logger) : BackgroundService
{
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    logger.LogInformation(
      "Worker started. Checking for due orders every {Seconds}s.", options.Value.PollIntervalSeconds);

    try
    {
      while (!stoppingToken.IsCancellationRequested)
      {
        var delay = options.Value.PollIntervalSeconds;
        try
        {
          await using var scope = scopeFactory.CreateAsyncScope();
          var processor = scope.ServiceProvider.GetRequiredService<OrderProcessor>();
          var changed = await processor.ProcessDueAsync(stoppingToken);
          logger.LogInformation("Polling completed. UpdatedOrders={Count}", changed);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
          throw;
        }
        catch (MongoAuthenticationException)
        {
          logger.LogCritical("MongoDB authentication failed. Check credentials; the worker is stopping.");
          throw;
        }
        catch (MongoException ex)
        {
          delay = options.Value.ErrorDelaySeconds;

          // Do not log the exception itself: it can contain the connection string.
          logger.LogError(
            "MongoDB operation failed ({ErrorType}). Retry in {DelaySeconds}s.", ex.GetType().Name, delay);
        }
        catch (TimeoutException)
        {
          delay = options.Value.ErrorDelaySeconds;
          logger.LogError(
            "MongoDB timeout. Check Atlas network access and cluster availability. Retry in {DelaySeconds}s.",
            delay);
        }

        await Task.Delay(TimeSpan.FromSeconds(delay), stoppingToken);
      }
    }
    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
    {
      logger.LogInformation("Worker cancellation requested.");
    }
    finally
    {
      logger.LogInformation("Worker stopped.");
    }
  }
}
