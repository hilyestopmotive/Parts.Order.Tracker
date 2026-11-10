using Microsoft.AspNetCore.Mvc;
using PartsOrderTracker.Api.Services;

namespace PartsOrderTracker.Api.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController(OrdersService ordersService) : ControllerBase
{
}
