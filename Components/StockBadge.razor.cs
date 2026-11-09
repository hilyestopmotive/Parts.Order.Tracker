namespace UI.Components;

using Microsoft.AspNetCore.Components;

public partial class StockBadge
{

  [Parameter]
  public bool InStock { get; set; }

  [Parameter]
  public int Quantity { get; set; }
}