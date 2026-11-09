namespace UI.Services;

using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using UI.DataType;

/// <summary>
/// Cart is persisted in the browser's local storage; orders are retrieved from and placed through the API.
/// </summary>
public class CartService(
  IHttpClientFactory httpClientFactory,
  AuthenticationStateProvider authStateProvider,
  IJSRuntime jsRuntime)
{
  private const string CartStorageKey = "cart";

  private static readonly JsonSerializerOptions ApiJsonOptions = new(JsonSerializerDefaults.Web);

  private readonly List<CartItem> items = [];

  private readonly List<Order> orders = [];

  private Task? initTask;

  private bool placingOrder;

  public event Action? Changed;

  public IReadOnlyList<CartItem> Items => items;

  public IReadOnlyList<Order> Orders => orders;

  public int Count => items.Sum(i => i.Quantity);

  public decimal Total => items.Sum(i => i.Total);

  public Task InitializeAsync() => initTask ??= LoadAsync();

  public void Add(Part part)
  {
    var existing = items.FirstOrDefault(i => i.PartId == part.Id);
    if (existing is null)
    {
      items.Add(new CartItem
      {
        PartId = part.Id,
        Name = part.Name,
        Brand = part.Manufacturer,
        Description = part.Description,
        PartNumber = part.ArticleNumber,
        UnitPrice = part.Price,
        ImageUrl = part.ImageUrl,
        Quantity = 1,
      });
    }
    else
    {
      existing.Quantity++;
    }

    OnCartChanged();
  }

  public void SetQuantity(string partId, int quantity)
  {
    var item = items.FirstOrDefault(i => i.PartId == partId);
    if (item is null)
    {
      return;
    }

    if (quantity <= 0)
    {
      items.Remove(item);
    }
    else
    {
      item.Quantity = quantity;
    }

    OnCartChanged();
  }

  public void Remove(string partId) => SetQuantity(partId, 0);

  public async Task<Order?> PlaceOrderAsync()
  {
    // Guard against double submission (e.g. a double-click) creating duplicate orders.
    if (placingOrder || items.Count == 0)
    {
      return null;
    }

    var user = (await authStateProvider.GetAuthenticationStateAsync()).User;
    if (user.Identity?.IsAuthenticated != true)
    {
      return null;
    }

    placingOrder = true;
    try
    {
      var client = httpClientFactory.CreateClient("PartsApi");

      // All cart items are placed as a single order.
      var request = new { items = items.Select(i => new { partId = i.PartId, quantity = i.Quantity }) };
      var response = await client.PostAsJsonAsync("api/orders", request);
      response.EnsureSuccessStatusCode();
      var placed = await response.Content.ReadFromJsonAsync<Order>(ApiJsonOptions);
      if (placed is null)
      {
        return null;
      }

      items.Clear();
      OnCartChanged();
      await LoadOrdersAsync();
      Changed?.Invoke();
      return orders.FirstOrDefault(o => o.Id == placed.Id) ?? placed;
    }
    finally
    {
      placingOrder = false;
    }
  }

  public async Task RefreshOrdersAsync()
  {
    await LoadOrdersAsync();
    Changed?.Invoke();
  }

  private async Task LoadOrdersAsync()
  {
    var user = (await authStateProvider.GetAuthenticationStateAsync()).User;
    if (user.Identity?.IsAuthenticated != true)
    {
      return;
    }

    var client = httpClientFactory.CreateClient("PartsApi");
    var loaded = await client.GetFromJsonAsync<List<Order>>("api/orders/my", ApiJsonOptions) ?? [];
    var parts = await client.GetFromJsonAsync<List<Part>>("api/parts", ApiJsonOptions) ?? [];

    foreach (var item in loaded.SelectMany(o => o.Items))
    {
      var part = parts.FirstOrDefault(p => p.Id == item.PartId);
      item.Description = part?.Description ?? string.Empty;
      item.ImageUrl = part?.ImageUrl ?? string.Empty;
      item.UnitPrice = part?.Price ?? 0;
    }

    orders.Clear();
    orders.AddRange(loaded);
  }

  private async Task LoadAsync()
  {
    var json = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", CartStorageKey);
    if (!string.IsNullOrWhiteSpace(json) && items.Count == 0)
    {
      items.AddRange(JsonSerializer.Deserialize<List<CartItem>>(json) ?? []);
    }

    try
    {
      await LoadOrdersAsync();
    }
    catch (HttpRequestException)
    {
    }

    Changed?.Invoke();
  }

  private void OnCartChanged()
  {
    _ = SaveAsync();
    Changed?.Invoke();
  }

  private async Task SaveAsync()
  {
    var json = JsonSerializer.Serialize(items);
    await jsRuntime.InvokeVoidAsync("localStorage.setItem", CartStorageKey, json);
  }
}
