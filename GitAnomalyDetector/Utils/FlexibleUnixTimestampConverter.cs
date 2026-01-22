using System.Text.Json;
using System.Text.Json.Serialization;

namespace GitAnomalyDetector.Utils
{
    public class FlexibleUnixTimestampConverter : JsonConverter<long?>
    {
        public override long? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Number:
                    if (reader.TryGetInt64(out long longValue))
                    {
                        return longValue;
                    }
                    break;

                case JsonTokenType.String:
                    string? stringValue = reader.GetString();
                    if (string.IsNullOrWhiteSpace(stringValue))
                    {
                        return null;
                    }

                    // Try to parse ISO 8601 string to Unix timestamp
                    if (DateTime.TryParse(stringValue, out DateTime dateTime))
                    {
                        return new DateTimeOffset(dateTime).ToUnixTimeSeconds();
                    }
                    break;

                case JsonTokenType.Null:
                    return null;
            }

            throw new JsonException($"Unable to convert \"{reader.GetString()}\" to Unix timestamp.");
        }

        public override void Write(Utf8JsonWriter writer, long? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteNumberValue(value.Value);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}
