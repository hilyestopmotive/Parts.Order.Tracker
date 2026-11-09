namespace UI.Components;

using System.Globalization;
using Microsoft.AspNetCore.Components;
using UI.DataType;

public partial class OrderCard
{
  private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("en-US");

  private bool expanded;

  [Parameter, EditorRequired]
  public Order Order { get; set; } = default!;

  private string StatusLabel => Order.Status switch
  {
    OrderStatus.Received => "Order Received",
    OrderStatus.Processing => "Processing",
    OrderStatus.Dispatched => "Dispatched",
    _ => Order.Status.ToString(),
  };

  private IEnumerable<TimelineStep> Steps =>
  [
    new("Order Received", Order.Status >= OrderStatus.Received, Order.ReceivedAt ?? Order.CreatedAt),
    new("Processing", Order.Status >= OrderStatus.Processing, Order.ProcessingAt),
    new("Dispatched", Order.Status >= OrderStatus.Dispatched, Order.DispatchedAt),
  ];

  private void Toggle() => expanded = !expanded;

  private sealed record TimelineStep(string Label, bool Done, DateTime? Date);
}
