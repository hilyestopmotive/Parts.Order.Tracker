namespace PartsOrderTracker.Api.Services;

public sealed class ClaimsCurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
  public string UserId => "11111111-1111-1111-1111-111111111111";
}
