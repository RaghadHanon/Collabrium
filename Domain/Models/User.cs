using Domain.Common;
using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Models;

public sealed class User : Entity
{
    public string? TenantId { get; private set; }

    public string Email { get; private set; } = string.Empty;
    public string UserName { get; private set; } = string.Empty;

    public string FullName { get; private set; } = null!;
    public Department? Department { get; private set; }

    public DateTime? LastLoginAt { get; private set; }
    public bool MustChangePassword { get; private set; }
    public DateOnly DateOfBirth { get; private set; }

    public ContactDetails ContactInfo { get; private set; } = ContactDetails.Empty;
    public StoredImage? ProfileImage { get; private set; }

    private User() { }

    public User(
        string id,
        string fullName,
        DateOnly dateOfBirth,
        string email = "",
        string userName = "",
        string? tenantId = null,
        Department? department = null)
        : base(id)
    {
        FullName = Guard.NotNullOrWhiteSpace(fullName, nameof(fullName));
        Guard.Against(dateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow), "dateOfBirth cannot be in the future.");

        DateOfBirth = dateOfBirth;
        Email = email?.Trim() ?? "";
        UserName = userName?.Trim() ?? "";
        TenantId = tenantId?.Trim();
        Department = department;

        MustChangePassword = true;
    }

    public static User Rehydrate(
        string id,
        DateTime createdAt,
        DateTime? updatedAt,
        string fullName,
        DateOnly dateOfBirth,
        string email,
        string userName,
        string? tenantId,
        Department? department,
        DateTime? lastLoginAt,
        bool mustChangePassword,
        ContactDetails contactInfo,
        StoredImage? profileImage)
    {
        var u = new User(id, fullName, dateOfBirth, email, userName, tenantId, department);
        u.SetTimestamps(createdAt, updatedAt);

        u.LastLoginAt = lastLoginAt;
        u.MustChangePassword = mustChangePassword;
        u.ContactInfo = contactInfo ?? ContactDetails.Empty;
        u.ProfileImage = profileImage;

        return u;
    }

    public void UpdateIdentity(string fullName, string email, string userName, DateOnly dateOfBirth)
    {
        FullName = Guard.NotNullOrWhiteSpace(fullName, nameof(fullName));
        Email = (email ?? "").Trim();
        UserName = (userName ?? "").Trim();
        Guard.Against(dateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-18),
            "User Must be at least 18.");
        DateOfBirth = dateOfBirth;

        Touch();
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        //MustChangePassword = false;
        Touch();
    }

    public void RequirePasswordChange()
    {
        MustChangePassword = true;
        Touch();
    }

    public void ChangeDepartment(Department? department)
    {
        Department = department;
        Touch();
    }

    public void ChangeTenant(string? tenantId)
    {
        TenantId = tenantId?.Trim();
        Touch();
    }

    public void SetContactInfo(string? email, string? phone, string? linkedIn, string? facebook,
        string? instagram, string? youTube, string? github)
    {
        var socials = SocialLinks.Create(linkedIn, facebook, instagram, youTube, github);
        ContactInfo = ContactDetails.Create(email, phone, socials);
        Touch();
    }

    public void SetProfileImage(string url, string publicId)
    {
        var image = StoredImage.Create(url, publicId);
        ProfileImage = image;
        Touch();
    }
}