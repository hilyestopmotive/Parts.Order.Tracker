namespace PartsOrderTracker.Api.Dtos;

public sealed record PartResponse(
  string Id,
  string ArticleNumber,
  string Name,
  string Manufacturer,
  decimal Price,
  bool InStock);
