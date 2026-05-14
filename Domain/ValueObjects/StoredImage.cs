using Domain.Core;

namespace Domain.ValueObjects;

public sealed record StoredImage(string Url, string PublicId)
{
    public static StoredImage Create(string url, string publicId)
    {
        return new(Guard.NotNullOrWhiteSpace(url, nameof(url)),
                   Guard.NotNullOrWhiteSpace(publicId, nameof(publicId)));
    }
}