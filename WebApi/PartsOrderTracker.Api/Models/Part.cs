using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PartsOrderTracker.Api.Models;

[BsonIgnoreExtraElements]
public sealed class Part
{
  [BsonId]
  public ObjectId Id { get; set; }

  [BsonElement("articleNumber")]
  public string ArticleNumber { get; set; } = string.Empty;

  [BsonElement("name")]
  public string Name { get; set; } = string.Empty;

  [BsonElement("manufacturer")]
  public string Manufacturer { get; set; } = string.Empty;

  [BsonElement("price")]
  public decimal Price { get; set; }

  [BsonElement("inStock")]
  public bool InStock { get; set; }

  [BsonElement("isActive")]
  public bool IsActive { get; set; }

  [BsonElement("category")]
  public string Category { get; set; } = string.Empty;

  [BsonElement("imageUrl")]
  public string ImageUrl { get; set; } = string.Empty;

  [BsonElement("description")]
  public string Description { get; set; } = string.Empty;
}
