namespace Domain.Models;

public class Tenant : Entity
{
    public string Name { get; private set; } = null!;
    public string Domain { get; private set; } = null!;
    public string? Description { get; private set; }
    public string? Country { get; private set; }
    public bool IsActive { get; private set; }
    public string? TenantAdminId { get; private set; }
    public string? WebsiteUrl { get; private set; }

    public TenantContactInfo? ContactInfo { get; private set; }
    public TenantImage? ProfileImage { get; private set; }

    public Tenant() { }

    public Tenant(string name, string domain)
    {
        Name = name;
        Domain = domain;
        IsActive = true;
        ContactInfo = new TenantContactInfo();
    }

    public Tenant(string id, string name, string domain)
    {
        Id = id;
        Name = name;
        Domain = domain;
        IsActive = true;
        ContactInfo = new TenantContactInfo();
    }

    public static Tenant Create(string name, string domain, string? description)
    {
        if (string.IsNullOrWhiteSpace(name)) 
            throw new ArgumentException("Tenant name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(domain))
            throw new ArgumentException("Tenant domain is required.", nameof(domain));

        var tenant = new Tenant(name.Trim(), domain.Trim());
        if (!string.IsNullOrWhiteSpace(description))
            tenant.Description = description;

        return tenant;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;

    public void UpdateProfile(string? description, string? country, string? websiteUrl)
    {
        Description = description;
        Country = country;
        WebsiteUrl = websiteUrl;
    }

    public void AssignAdmin(string adminUserId)
    {
        if (string.IsNullOrWhiteSpace(adminUserId))
            throw new ArgumentException("Admin user id is required.", nameof(adminUserId));

        TenantAdminId = adminUserId;
    }

    public void UpdateContactInfo(string? email, string? phoneNumber, 
        string? linkedIn, string? facebook, string? instagram, string? youTube)
    {
        if (email is null && phoneNumber is null) return;

        var newEmail = email ?? ContactInfo!.Email;
        var newPhone = phoneNumber ?? ContactInfo!.PhoneNumber;
        var newLinkedIn = linkedIn ?? ContactInfo!.LinkedIn;
        var newFacebook = facebook ?? ContactInfo!.Facebook;
        var newInstagram = instagram ?? ContactInfo!.Instagram;
        var newYouTube = youTube ?? ContactInfo!.YouTube;

        ContactInfo!.UpdateContact(newEmail, newPhone);
        ContactInfo!.UpdateSocials(newLinkedIn, newFacebook, newInstagram, newYouTube);
    }

    public void SetProfileImage(string url, string publicId)
    {
        if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("Url is required.", nameof(url));
        if (string.IsNullOrWhiteSpace(publicId)) throw new ArgumentException("PublicId is required.", nameof(publicId));

        ProfileImage = new TenantImage(url, publicId);
    }

    public void RemoveProfileImage()
    {
        ProfileImage = null;
    }

    public void SetContactInfo(TenantContactInfo? info) => ContactInfo = info;
    public void SetProfileImage(TenantImage? image) => ProfileImage = image;
}