using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PartsOrderTracker.Worker.Models;

// The worker's view of an order document. Only the fields the worker reads or writes are
// mapped; everything else the API stores (items, user info, ...) is ignored and left untouched.
[BsonIgnoreExtraElements]
public sealed class Order
{
  [BsonId]
  public ObjectId Id { get; set; }

  [BsonElement("userId")]
  public string UserId { get; set; } = string.Empty;

  [BsonElement("status")]
  [BsonRepresentation(BsonType.String)]
  public OrderStatus Status { get; set; }

  [BsonElement("updatedAt")]
  [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
  public DateTime UpdatedAt { get; set; }

  [BsonElement("nextStatusChangeAt")]
  [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
  public DateTime? NextStatusChangeAt { get; set; }

  [BsonElement("processingAt")]
  [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
  public DateTime? ProcessingAt { get; set; }

  [BsonElement("dispatchedAt")]
  [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
  public DateTime? DispatchedAt { get; set; }
}
