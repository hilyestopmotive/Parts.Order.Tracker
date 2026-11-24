// LOGIN STEP (API 5/5): using for GetObjectId() (reads the "oid" claim from the token)
//using Microsoft.Identity.Web;

namespace PartsOrderTracker.Api.Services;

public sealed class ClaimsCurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
  // BEFORE LOGIN: a fixed test user, so every order belongs to the same user.
  // LOGIN STEP (API 5/5): delete these 3 properties and uncomment the 3 below.
  public string UserId => "11111111-1111-1111-1111-111111111111";

  public string? UserName => "Test User";

  public string? Email => "testuser@example.com";

  // LOGIN STEP (API 5/5): read the logged-in user from the access token.
  // Only works after API 1/5 - 4/5: without a validated token there is no "oid" claim.
  //public string UserId =>
  //  httpContextAccessor.HttpContext?.User.GetObjectId()
  //  ?? throw new InvalidOperationException("No logged-in user with an object id (oid) claim.");

  //public string? UserName =>
  //  httpContextAccessor.HttpContext?.User.FindFirst("name")?.Value ?? string.Empty;

  //public string? Email =>
  //  httpContextAccessor.HttpContext?.User.FindFirst("preferred_username")?.Value
  //  ?? httpContextAccessor.HttpContext?.User.FindFirst("email")?.Value
  //  ?? string.Empty;
}
