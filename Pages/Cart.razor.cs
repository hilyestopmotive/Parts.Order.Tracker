namespace UI.Pages;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using UI.DataType;
using UI.Services;

public partial class Cart : IDisposable
{
  private Order? orderPlaced;

  private string? orderError;

  private bool isAuthenticated;

  [Inject]
  private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

  [Inject]
  private CartService CartService { get; set; } = default!;

  protected override async Task OnInitializedAsync()
  {
    CartService.Changed += StateHasChanged;
    await CartService.InitializeAsync();
    isAuthenticated = (await AuthStateProvider.GetAuthenticationStateAsync()).User.Identity?.IsAuthenticated == true;
  }

  public void Dispose() => CartService.Changed -= StateHasChanged;

  private int inputVersion;

  private void ChangeQuantity(string partId, ChangeEventArgs e)
  {
    var quantity = int.TryParse(e.Value?.ToString(), out var parsed) ? Math.Max(1, parsed) : 1;
    CartService.SetQuantity(partId, quantity);
    inputVersion++;
  }

  private async Task PlaceOrder()
  {
    orderError = null;
    try
    {
      orderPlaced = await CartService.PlaceOrderAsync();
    }
    catch (HttpRequestException ex)
    {
      orderError = $"Placing the order failed: {ex.Message}";
    }
  }
}
