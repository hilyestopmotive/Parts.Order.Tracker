using Microsoft.AspNetCore.Mvc;
using PartsOrderTracker.Api.Services;

namespace PartsOrderTracker.Api.Controllers;

[ApiController]
[Route("api/parts")]
public sealed class PartsController(PartsService partsService) : ControllerBase
{
  private readonly PartsService _partsService = partsService;
}
