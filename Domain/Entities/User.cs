using Domain.Enums;

namespace Domain.Models;

public class User : Entity
{
    public string? TenantId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string UserName { get; private set; } = string.Empty;

    public string FullName { get; private set; } = null!;
    public Department? Department { get; private set; }

    public DateTime? LastLoginAt { get; private set; }
    public bool MustChangePassword { get; private set; }
    public DateOnly DateOfBirth { get; private set; }

    public int Age
    {
        get
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var age = today.Year - DateOfBirth.Year;
            if (DateOfBirth > today.AddYears(-age)) age--;
            return age;
        }
    }

    public UserContactInfo? ContactInfo { get; private set; }
    public UserImage? ProfileImage { get; private set; }

    public User() { }

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
        FullName = fullName;
        DateOfBirth = dateOfBirth;
        Email = email;
        UserName = userName;
        TenantId = tenantId;
        Department = department;
        MustChangePassword = false;
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        MustChangePassword = false;
    }

    public void RequirePasswordChange()
    {
        MustChangePassword = true;
    }

    public void ChangeDepartment(Department? department)
    {
        Department = department;
    }

    public void SetContactInfo(UserContactInfo? contactInfo)
    {
        ContactInfo = contactInfo;
    }

    public void SetProfileImage(UserImage? image)
    {
        ProfileImage = image;
    }
}
