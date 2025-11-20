using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using FunnyActivities.Domain.ValueObjects;

namespace FunnyActivities.CrossCuttingConcerns.Serialization
{
    /// <summary>
    /// JSON converter that serializes the VideoUrl value object as a simple string
    /// so that cached entities can be round-tripped when using System.Text.Json.
    /// </summary>
    public class VideoUrlJsonConverter : JsonConverter<VideoUrl?>
    {
        public override VideoUrl? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            string? value = null;

            if (reader.TokenType == JsonTokenType.String)
            {
                value = reader.GetString();
            }
            else if (reader.TokenType == JsonTokenType.StartObject)
            {
                using var document = JsonDocument.ParseValue(ref reader);
                if (document.RootElement.TryGetProperty("Value", out var valueProperty))
                {
                    value = valueProperty.GetString();
                }
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return VideoUrl.Create(value);
        }

        public override void Write(Utf8JsonWriter writer, VideoUrl? value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStringValue(value.Value);
        }
    }
}
