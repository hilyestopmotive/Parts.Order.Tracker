namespace UI.Components;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using UI.DataType;
using UI.Services;

public partial class PartCard : IDisposable
{
  private static readonly string[] FeatureIcons = ["bi-box-seam", "bi-shield-check", "bi-car-front"];

  [Inject]
  private CartService CartService { get; set; } = default!;

  [Inject]
  private FavoritesService FavoritesService { get; set; } = default!;

  [Inject]
  private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

  private bool isAuthenticated;

  [Parameter, EditorRequired]
  public Part Part { get; set; } = default!;

  private bool IsFavorite => FavoritesService.IsFavorite(Part.Id);

  protected override async Task OnInitializedAsync()
  {
    FavoritesService.Changed += OnChanged;
    await FavoritesService.InitializeAsync();
    isAuthenticated = (await AuthStateProvider.GetAuthenticationStateAsync()).User.Identity?.IsAuthenticated == true;
  }

  private void OnChanged() => InvokeAsync(StateHasChanged);

  public void Dispose() => FavoritesService.Changed -= OnChanged;
}
