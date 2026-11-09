namespace UI.DataType;

public class Part
{
  public string Id { get; set; } = string.Empty;
  public string ArticleNumber { get; set; } = string.Empty;

  public string Name { get; set; } = string.Empty;

  public string Manufacturer { get; set; } = string.Empty;

  public string Description { get; set; } = string.Empty;

  public string Category { get; set; } = string.Empty;

  public decimal Price { get; set; }

  public string ImageUrl { get; set; } = string.Empty;

  public bool InStock { get; set; }

  public bool IsActive { get; set; }
}