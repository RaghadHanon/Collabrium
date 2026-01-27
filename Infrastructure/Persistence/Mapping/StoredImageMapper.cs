using Domain.ValueObjects;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mapping;

public static class StoredImageMapper
{
    public static StoredImage? ToDomain(StoredImageOwned? owned)
    {
        return owned is null
            ? null
            : string.IsNullOrWhiteSpace(owned.Url) || string.IsNullOrWhiteSpace(owned.PublicId)
            ? null
            : StoredImage.Create(owned.Url, owned.PublicId);
    }

    public static StoredImageOwned? ToOwned(StoredImage? domain)
    {
        return domain is null
            ? null
            : new StoredImageOwned
            {
                Url = domain.Url,
                PublicId = domain.PublicId
            };
    }

    public static void UpdateOwned(StoredImage? domain, StoredImageOwned owned)
    {
        if (owned is null) throw new ArgumentNullException(nameof(owned));

        if (domain is null)
        {
            owned.Url = string.Empty;
            owned.PublicId = string.Empty;
            return;
        }

        owned.Url = domain.Url;
        owned.PublicId = domain.PublicId;
    }
}
