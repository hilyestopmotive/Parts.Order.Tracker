namespace UI.Components;

using Microsoft.AspNetCore.Components;
using UI.Services;

public partial class CartBadge : IDisposable
{
  [Inject]
  private CartService CartService { get; set; } = default!;

  protected override async Task OnInitializedAsync()
  {
    CartService.Changed += OnChanged;
    await CartService.InitializeAsync();
  }

  private void OnChanged() => InvokeAsync(StateHasChanged);

  public void Dispose() => CartService.Changed -= OnChanged;
}
