namespace UI.DataType;

using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrderStatus
{
  Received,
  Processing,
  Dispatched,
}
