using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace API.Common.EnumHandlers;
public class SpacedEnumConverter<T> : JsonConverter<T> where T : struct, Enum
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();

        if (string.IsNullOrWhiteSpace(value))
            throw new JsonException("Enum value cannot be empty.");

        var normalized = value.Replace(" ", "");

        if (Enum.TryParse<T>(normalized, true, out var result))
            return result;

        throw new JsonException($"Invalid value '{value}' for enum {typeof(T).Name}");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        var stringValue = value.ToString();
        var spaced = Regex.Replace(value.ToString(), "(\\B[A-Z])", " $1");

        writer.WriteStringValue(stringValue.Length > 3 ? spaced : stringValue);
    }
}