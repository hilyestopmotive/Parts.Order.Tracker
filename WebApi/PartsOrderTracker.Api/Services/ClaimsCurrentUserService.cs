using Microsoft.Identity.Web;

namespace PartsOrderTracker.Api.Services;

// The logged-in user, read from the validated Entra ID access token.
public sealed class ClaimsCurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
  // "oid" = the user's unique object id in Entra ID; orders are saved and filtered by it.
  public string UserId =>
    httpContextAccessor.HttpContext?.User.GetObjectId()
    ?? throw new InvalidOperationException("No logged-in user with an object id (oid) claim.");

  public string? UserName =>
    httpContextAccessor.HttpContext?.User.FindFirst("name")?.Value ?? string.Empty;

  public string? Email =>
    httpContextAccessor.HttpContext?.User.FindFirst("preferred_username")?.Value
    ?? httpContextAccessor.HttpContext?.User.FindFirst("email")?.Value
    ?? string.Empty;
}
