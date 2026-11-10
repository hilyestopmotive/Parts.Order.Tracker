namespace UI.Pages;

using Microsoft.AspNetCore.Components;
using UI.Services;

public partial class Orders : IDisposable
{
  private bool loaded;

  [Inject]
  private CartService CartService { get; set; } = default!;

  [Inject]
  private OrderNotificationService Notifications { get; set; } = default!;

  protected override async Task OnInitializedAsync()
  {
    CartService.Changed += OnOrdersChanged;
    await CartService.InitializeAsync();
    await Notifications.StartAsync();
    loaded = true;
  }

  private void OnOrdersChanged() => InvokeAsync(StateHasChanged);

  public void Dispose() => CartService.Changed -= OnOrdersChanged;
}
