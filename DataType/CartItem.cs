namespace UI.DataType;

public class CartItem
{
  [System.Text.Json.Serialization.JsonConverter(typeof(FlexibleStringConverter))]
  public string PartId { get; set; } = string.Empty;

  public string Name { get; set; } = string.Empty;

  public string Brand { get; set; } = string.Empty;

  public string Description { get; set; } = string.Empty;

  public string PartNumber { get; set; } = string.Empty;

  public decimal UnitPrice { get; set; }

  public string ImageUrl { get; set; } = string.Empty;

  public int Quantity { get; set; }

  public decimal Total => UnitPrice * Quantity;
}
