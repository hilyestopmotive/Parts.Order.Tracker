namespace UI.Pages;

using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using UI.DataType;

public partial class Parts
{
  private List<Part>? parts;

  [Inject]
  private IHttpClientFactory HttpClientFactory { get; set; } = default!;

  [Inject]
  private NavigationManager Navigation { get; set; } = default!;

  private string SearchText { get; set; } = string.Empty;

  private string SelectedCategory { get; set; } = string.Empty;

  private string SortOption { get; set; } = "popular";

  private IEnumerable<string> Categories =>
    (parts ?? []).Select(p => p.Category).Where(c => c.Length > 0).Distinct().Order();

  private IEnumerable<Part> FilteredParts
  {
    get
    {
      var query = (parts ?? []).AsEnumerable();

      if (SelectedCategory.Length > 0)
      {
        query = query.Where(p => p.Category == SelectedCategory);
      }

      var text = SearchText.Trim();
      if (text.Length > 0)
      {
        query = query.Where(p =>
          p.Name.Contains(text, StringComparison.OrdinalIgnoreCase)
          || p.Manufacturer.Contains(text, StringComparison.OrdinalIgnoreCase)
          || p.ArticleNumber.Contains(text, StringComparison.OrdinalIgnoreCase)
          || p.Description.Contains(text, StringComparison.OrdinalIgnoreCase));
      }

      return SortOption switch
      {
        "price-asc" => query.OrderBy(p => p.Price),
        "price-desc" => query.OrderByDescending(p => p.Price),
        "name" => query.OrderBy(p => p.Name),
        _ => query.OrderByDescending(p => p.IsActive),
      };
    }
  }

  protected override async Task OnInitializedAsync()
  {
    try
    {
      var client = HttpClientFactory.CreateClient("PartsApi");
      parts = await client.GetFromJsonAsync<List<Part>>("api/parts");
    }
    catch (AccessTokenNotAvailableException ex)
    {
      ex.Redirect();
    }
  }
}
