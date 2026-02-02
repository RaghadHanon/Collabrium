using Domain.Common;
using Domain.ValueObjects;

namespace Domain.Models;

public sealed class Tenant : Entity
{
    public string Name { get; private set; } = null!;
    public string Domain { get; private set; } = null!;
    public bool IsActive { get; private set; }

    public string? Description { get; private set; }
    public string? Country { get; private set; }
    public string? WebsiteUrl { get; private set; }
    public string? TenantAdminId { get; private set; }

    public ContactDetails ContactInfo { get; private set; } = ContactDetails.Empty;
    public StoredImage? ProfileImage { get; private set; }

    private Tenant() { } // EF optional

    public Tenant(string name, string domain)
    {
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name));
        Domain = Guard.NotNullOrWhiteSpace(domain, nameof(domain)).ToLowerInvariant();
        IsActive = true;
    }

    public Tenant(string id, string name, string domain) : base(id)
    {
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name));
        Domain = Guard.NotNullOrWhiteSpace(domain, nameof(domain)).ToLowerInvariant();
        IsActive = true;
    }

    public static Tenant Rehydrate(
        string id,
        DateTime createdAt,
        DateTime? updatedAt,
        string name,
        string domain,
        bool isActive,
        string? description,
        string? country,
        string? websiteUrl,
        string? tenantAdminId,
        ContactDetails contactInfo,
        StoredImage? profileImage)
    {
        var t = new Tenant(id, name, domain);
        t.SetTimestamps(createdAt, updatedAt);

        t.IsActive = isActive;
        t.Description = description?.Trim();
        t.Country = country?.Trim();
        t.WebsiteUrl = websiteUrl?.Trim();
        t.TenantAdminId = tenantAdminId?.Trim();
        t.ContactInfo = contactInfo ?? ContactDetails.Empty;
        t.ProfileImage = profileImage;

        return t;
    }

    public void UpdateProfile(string? description, string? country, string? websiteUrl)
    {
        Description = description?.Trim();
        Country = country?.Trim();
        WebsiteUrl = websiteUrl?.Trim();
        Touch();
    }

    public void AssignAdmin(string adminId)
    {
        TenantAdminId = Guard.NotNullOrWhiteSpace(adminId, nameof(adminId));
        Touch();
    }

    public void Activate() { IsActive = true; Touch(); }
    public void Deactivate() { IsActive = false; Touch(); }

    public void SetProfileImage(string url, string publicId)
    {
        var image = StoredImage.Create(url, publicId);
        ProfileImage = image ?? throw new DomainException("Image is required.");
        Touch();
    }

    public void RemoveProfileImage()
    {
        ProfileImage = null;
        Touch();
    }

    public void SetContactInfo(string? email, string? phone, string? linkedIn, string? facebook,
        string? instagram, string? youTube)
    {
        var socials = SocialLinks.Create(linkedIn, facebook, instagram, youTube);
        ContactInfo = ContactDetails.Create(email, phone, socials);
        Touch();
    }
}