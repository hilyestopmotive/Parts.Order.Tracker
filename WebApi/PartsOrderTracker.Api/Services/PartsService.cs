using MongoDB.Bson;
using MongoDB.Driver;
using PartsOrderTracker.Api.Dtos;
using PartsOrderTracker.Api.Models;

namespace PartsOrderTracker.Api.Services;

public sealed class PartsService(IMongoDatabase database)
{
  private readonly IMongoCollection<Part> _partsCollection =
    database.GetCollection<Part>("parts");

  public async Task<IEnumerable<PartResponse>> GetActiveAsync
    (CancellationToken cancellationToken)
  {
    var parts = await _partsCollection
      .Find(part => part.IsActive)
      .SortBy(part => part.Name)
      .ToListAsync(cancellationToken);

    return parts.ConvertAll(ToResponse);
  }

  public async Task<PartResponse?> GetActiveByIdAsync
    (ObjectId id, CancellationToken cancellationToken)
  {
    var part = await _partsCollection
      .Find(part => part.IsActive && part.Id == id)
      .FirstOrDefaultAsync(cancellationToken);

    if(part == null)
    {
      return null;
    }
    else
    {
      return ToResponse(part);
    }
  }

  private PartResponse ToResponse(Part part)
  {
    return new PartResponse(
      part.Id.ToString(),
      part.ArticleNumber,
      part.Name,
      part.Manufacturer,
      part.Price,
      part.InStock,
      part.Category,
      part.ImageUrl,
      part.Description);
  }
}