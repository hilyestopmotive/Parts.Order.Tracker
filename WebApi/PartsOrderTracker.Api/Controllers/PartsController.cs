using Microsoft.AspNetCore.Mvc;
using PartsOrderTracker.Api.Dtos;
using PartsOrderTracker.Api.Services;

namespace PartsOrderTracker.Api.Controllers;

[ApiController]
[Route("api/parts")]
public sealed class PartsController(PartsService partsService) :
  ControllerBase
{
  [HttpGet]
  [ProducesResponseType<IReadOnlyList<PartResponse>>
    (StatusCodes.Status200OK)]
  public async Task<ActionResult<IReadOnlyList<PartResponse>>> 
    GetActive(CancellationToken cancellationToken)
  {
    var parts = await partsService.GetActiveAsync(cancellationToken);
    return Ok(parts);
  }
}
