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

    public override JsonConverter CreateConverter(Type type, JsonSerializerOptions options)
    {
        var enumType = Nullable.GetUnderlyingType(type) ?? type;

        var converterType = typeof(SpacedEnumConverter<>).MakeGenericType(enumType);

        return (JsonConverter) Activator.CreateInstance(converterType)!;
    }
}