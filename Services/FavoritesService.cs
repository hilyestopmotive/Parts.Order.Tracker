namespace UI.Services;

using System.Text.Json;
using Microsoft.JSInterop;

public class FavoritesService(IJSRuntime jsRuntime)
{
  private const string StorageKey = "favorites";

  private readonly HashSet<string> ids = [];

  private Task? initTask;

  public event Action? Changed;

  public Task InitializeAsync() => initTask ??= LoadAsync();

  public bool IsFavorite(string partId) => ids.Contains(partId);

  public void Toggle(string partId)
  {
    if (!ids.Remove(partId))
    {
      ids.Add(partId);
    }

    _ = SaveAsync();
    Changed?.Invoke();
  }

  private async Task LoadAsync()
  {
    var json = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", StorageKey);
    if (!string.IsNullOrWhiteSpace(json))
    {
      foreach (var id in JsonSerializer.Deserialize<List<string>>(json) ?? [])
      {
        ids.Add(id);
      }
    }

    Changed?.Invoke();
  }

  private async Task SaveAsync()
  {
    var json = JsonSerializer.Serialize(ids);
    await jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
  }
}
