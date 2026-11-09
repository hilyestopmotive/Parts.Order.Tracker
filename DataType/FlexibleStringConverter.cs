namespace UI.DataType;

using System.Text.Json;
using System.Text.Json.Serialization;

public class FlexibleStringConverter : JsonConverter<string>
{
  public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
    reader.TokenType switch
    {
      JsonTokenType.String => reader.GetString() ?? string.Empty,
      JsonTokenType.Number => reader.GetInt64().ToString(),
      JsonTokenType.Null => string.Empty,
      _ => throw new JsonException($"Unexpected token {reader.TokenType}."),
    };

  public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) =>
    writer.WriteStringValue(value);
}
