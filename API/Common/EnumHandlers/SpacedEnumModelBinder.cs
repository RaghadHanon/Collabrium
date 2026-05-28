using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace API.Common.EnumHandlers;

public sealed class SpacedEnumModelBinder<TEnum> : IModelBinder
    where TEnum : struct, Enum
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

        if (valueResult == ValueProviderResult.None)
            return Task.CompletedTask;

        bindingContext.ModelState.SetModelValue(
            bindingContext.ModelName,
            valueResult);

        var rawValue = valueResult.FirstValue;

        if (string.IsNullOrWhiteSpace(rawValue))
            return Task.CompletedTask;

        if (Enum.TryParse<TEnum>(rawValue, ignoreCase: true, out var directResult))
        {
            bindingContext.Result = ModelBindingResult.Success(directResult);
            return Task.CompletedTask;
        }

        var normalizedValue = Normalize(rawValue);

        foreach (var enumName in Enum.GetNames<TEnum>())
        {
            if (string.Equals(
                    Normalize(enumName),
                    normalizedValue,
                    StringComparison.OrdinalIgnoreCase))
            {
                var enumValue = Enum.Parse<TEnum>(enumName);
                bindingContext.Result = ModelBindingResult.Success(enumValue);
                return Task.CompletedTask;
            }
        }

        bindingContext.ModelState.TryAddModelError(
            bindingContext.ModelName,
            $"The value '{rawValue}' is not valid for {bindingContext.ModelName}.");

        return Task.CompletedTask;
    }

    private static string Normalize(string value)
    {
        return value
            .Replace(" ", string.Empty)
            .Replace("-", string.Empty)
            .Replace("_", string.Empty);
    }
}