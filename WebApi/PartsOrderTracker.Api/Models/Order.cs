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

  [BsonElement("part")]
  [BsonIgnoreIfNull]
  public OrderItem? LegacyPart { get; set; }

  [BsonElement("quantity")]
  [BsonIgnoreIfNull]
  public int? LegacyQuantity { get; set; }

  [BsonElement("status")]
  [BsonRepresentation(BsonType.String)]
  public OrderStatus Status { get; set; }

  [BsonElement("createdAt")]
  [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
  public DateTime CreatedAt { get; set; }

  [BsonElement("updatedAt")]
  [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
  public DateTime UpdatedAt { get; set; }

  [BsonElement("nextStatusChangeAt")]
  [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
  public DateTime? NextStatusChangeAt { get; set; }

  [BsonElement("userName")]
  public string UserName { get; set; } = string.Empty;

  [BsonElement("userEmail")]
  public string UserEmail { get; set; } = string.Empty;

  [BsonElement("receivedAt")]
  [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
  public DateTime? ReceivedAt { get; set; }

  [BsonElement("processingAt")]
  [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
  public DateTime? ProcessingAt { get; set; }

  [BsonElement("dispatchedAt")]
  [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
  public DateTime? DispatchedAt { get; set; }

  [BsonElement("expectedArrival")]
  [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
  public DateTime? ExpectedArrival { get; set; }

  [BsonElement("trackingId")]
  public string TrackingId { get; set; } = string.Empty;
}

[BsonIgnoreExtraElements]
public sealed class OrderItem
{
  [BsonElement("partId")]
  [BsonIgnoreIfNull]
  public ObjectId? PartId { get; set; }

  [BsonElement("articleNumber")]
  public string ArticleNumber { get; set; } = string.Empty;

  [BsonElement("name")]
  public string Name { get; set; } = string.Empty;

  [BsonElement("manufacturer")]
  public string Manufacturer { get; set; } = string.Empty;

  [BsonElement("quantity")]
  public int Quantity { get; set; }
}
