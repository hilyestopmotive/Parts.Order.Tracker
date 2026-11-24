using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace UI.Services;

// TEMPORARY - only needed until the Login step.
// The app's components ask "who is the current user?" (AuthenticationStateProvider).
// Before login is added, this answers "nobody is logged in", so the app can start without Entra ID.
// LOGIN STEP: once AddMsalAuthentication is enabled in Program.cs, this class is no longer used and can be deleted.
public sealed class AnonymousAuthenticationStateProvider : AuthenticationStateProvider
{
  private static readonly Task<AuthenticationState> Anonymous =
    Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));

  public override Task<AuthenticationState> GetAuthenticationStateAsync() => Anonymous;
}
