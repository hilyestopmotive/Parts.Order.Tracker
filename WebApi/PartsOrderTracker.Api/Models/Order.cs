using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PartsOrderTracker.Api.Models;

[BsonIgnoreExtraElements]
public sealed class Order
{
  [BsonId]
  public ObjectId Id { get; set; }

  [BsonElement("userId")]
  public string UserId { get; set; } = string.Empty;

  [BsonElement("items")]
  public List<OrderItem> Items { get; set; } = [];

  [BsonElement("status")]
  [BsonRepresentation(BsonType.String)]
  public OrderStatus Status { get; set; }

  [BsonElement("createdAt")]
  [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
  public DateTime CreatedAt { get; set; }
}

[BsonIgnoreExtraElements]
public sealed class OrderItem
{
  [BsonElement("partId")]
  public ObjectId PartId { get; set; }

  [BsonElement("articleNumber")]
  public string ArticleNumber { get; set; } = string.Empty;

  [BsonElement("name")]
  public string Name { get; set; } = string.Empty;

  [BsonElement("quantity")]
  public int Quantity { get; set; }
}
