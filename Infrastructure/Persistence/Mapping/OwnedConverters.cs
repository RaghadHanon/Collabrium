using Domain.ValueObjects;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mapping;

public static class OwnedConverters
{
    public static SocialLinks ToDomain(this SocialLinksOwned o)
    {
        return SocialLinks.Create(o.LinkedIn, o.Facebook, o.Instagram, o.YouTube);
    }

    public static SocialLinksOwned ToOwned(this SocialLinks v)
    {
        return new()
        {
            LinkedIn = v.LinkedIn ?? "",
            Facebook = v.Facebook ?? "",
            Instagram = v.Instagram ?? "",
            YouTube = v.YouTube ?? ""
        };
    }

    public static ContactDetails ToDomain(this ContactDetailsOwned o)
    {
        return ContactDetails.Create(o.Email, o.PhoneNumber, o.Socials.ToDomain());
    }

    public static ContactDetailsOwned ToOwned(this ContactDetails v)
    {
        return new()
        {
            Email = v.Email ?? "",
            PhoneNumber = v.PhoneNumber ?? "",
            Socials = v.Socials.ToOwned()
        };
    }

    public static StoredImage? ToDomain(this StoredImageOwned? o)
    {
        return o is null ? null : StoredImage.Create(o.Url, o.PublicId);
    }

    public static StoredImageOwned? ToOwned(this StoredImage? v)
    {
        return v is null ? null : new StoredImageOwned { Url = v.Url, PublicId = v.PublicId };
    }
}
