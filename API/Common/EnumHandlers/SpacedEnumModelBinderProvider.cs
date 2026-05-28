using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace API.Common.EnumHandlers;

public sealed class SpacedEnumModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        var type = Nullable.GetUnderlyingType(context.Metadata.ModelType)
                   ?? context.Metadata.ModelType;

        if (!type.IsEnum)
            return null;

        var binderType = typeof(SpacedEnumModelBinder<>).MakeGenericType(type);

        return (IModelBinder) Activator.CreateInstance(binderType)!;
    }
}
