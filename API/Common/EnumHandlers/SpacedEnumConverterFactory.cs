using System.Text.Json;
using System.Text.Json.Serialization;

namespace API.Common.EnumHandlers;

public class SpacedEnumConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        var type = Nullable.GetUnderlyingType(typeToConvert) ?? typeToConvert;
        return type.IsEnum;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var underlyingEnum = Nullable.GetUnderlyingType(typeToConvert);

        if (underlyingEnum != null)
        {
            var converterType = typeof(NullableSpacedEnumConverter<>).MakeGenericType(underlyingEnum);
            return (JsonConverter) Activator.CreateInstance(converterType)!;
        }

        var enumConverter = typeof(SpacedEnumConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter) Activator.CreateInstance(enumConverter)!;
    }
}

public class NullableSpacedEnumConverter<T> : JsonConverter<T?> where T : struct, Enum
{
    private readonly SpacedEnumConverter<T> _converter = new();

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        return _converter.Read(ref reader, typeof(T), options);
    }

    public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        _converter.Write(writer, value.Value, options);
    }
}