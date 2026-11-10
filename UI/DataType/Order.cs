namespace UI.DataType;

using System.Text.Json.Serialization;

public class Order
{
  public string Id { get; set; } = string.Empty;

  public string OrderNumber => $"ORD-{(Id.Length > 6 ? Id[^6..] : Id).ToUpperInvariant()}";

  public List<OrderItem> Items { get; set; } = [];

  public OrderStatus Status { get; set; }

  public DateTime CreatedAt { get; set; }

  public DateTime UpdatedAt { get; set; }

  public DateTime? NextStatusChangeAt { get; set; }

  public string UserName { get; set; } = string.Empty;

  public string UserEmail { get; set; } = string.Empty;

  public DateTime? ReceivedAt { get; set; }

  public DateTime? ProcessingAt { get; set; }

  public DateTime? DispatchedAt { get; set; }

  public DateTime? ExpectedArrival { get; set; }

  public string TrackingId { get; set; } = string.Empty;

  public int ProductCount => Items.Count;

  public int ItemCount => Items.Sum(i => i.Quantity);

  public decimal Total => Items.Sum(i => i.Total);
}

public class OrderItem
{
  public string? PartId { get; set; }

  public string ArticleNumber { get; set; } = string.Empty;

  public string Name { get; set; } = string.Empty;

  public string Manufacturer { get; set; } = string.Empty;

  public int Quantity { get; set; }

  [JsonIgnore]
  public string Description { get; set; } = string.Empty;

  [JsonIgnore]
  public string ImageUrl { get; set; } = string.Empty;

  [JsonIgnore]
  public decimal UnitPrice { get; set; }

  public decimal Total => UnitPrice * Quantity;
}
