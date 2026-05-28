using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace API.Common.EnumHandlers;

public class SpacedEnumConverter<T> : JsonConverter<T> where T : struct, Enum
{
    public override T Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var value = reader.GetString();

        if (string.IsNullOrWhiteSpace(value))
            throw new JsonException("Enum value cannot be empty.");

        var normalizedValue = Normalize(value);

        foreach (var enumName in Enum.GetNames<T>())
        {
            if (string.Equals(
                    Normalize(enumName),
                    normalizedValue,
                    StringComparison.OrdinalIgnoreCase))
            {
                return Enum.Parse<T>(enumName);
            }
        }

        throw new JsonException($"Invalid value '{value}' for enum {typeof(T).Name}");
    }

    public override void Write(
        Utf8JsonWriter writer,
        T value,
        JsonSerializerOptions options)
    {
        var spaced = Regex.Replace(value.ToString(), "(\\B[A-Z])", " $1");
        writer.WriteStringValue(spaced);
    }

    private static string Normalize(string value)
    {
        return value
            .Replace(" ", string.Empty)
            .Replace("-", string.Empty)
            .Replace("_", string.Empty);
    }
}