using PartsOrderTracker.Api.Dtos;
using PartsOrderTracker.Api.Models;

namespace PartsOrderTracker.Api.Services;

public sealed class PartsService
{
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