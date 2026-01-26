using Domain.Entities;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mapping;

public static class UserMapper
{
    public static User ToDomain(ApplicationUserEntity entity)
    {
        return entity is null
            ? throw new ArgumentNullException(nameof(entity))
            : User.Rehydrate(
            id: entity.Id,
            createdAt: entity.CreatedAt,
            updatedAt: entity.UpdatedAt,
            fullName: entity.FullName,
            dateOfBirth: entity.DateOfBirth,
            email: entity.Email ?? string.Empty,
            userName: entity.UserName ?? string.Empty,
            tenantId: entity.TenantId,
            department: entity.Department,
            lastLoginAt: entity.LastLoginAt,
            mustChangePassword: entity.MustChangePassword,
            contactInfo: entity.ContactInfo.ToDomain(),
            profileImage: entity.ProfileImage.ToDomain()
        );
    }

    /// <summary>
    /// يفضل استخدام UpdateEntity بدل ToEntity لأن IdentityUser فيه حقول كثيرة
    /// (PasswordHash, SecurityStamp, ConcurrencyStamp...)
    /// </summary>
    public static ApplicationUserEntity ToEntity(User domain)
    {
        return domain is null
            ? throw new ArgumentNullException(nameof(domain))
            : new ApplicationUserEntity
            {
                Id = domain.Id,
                CreatedAt = domain.CreatedAt,
                UpdatedAt = domain.UpdatedAt,

                TenantId = domain.TenantId,
                FullName = domain.FullName,
                Department = domain.Department,

                DateOfBirth = domain.DateOfBirth,
                LastLoginAt = domain.LastLoginAt,
                MustChangePassword = domain.MustChangePassword,

                Email = domain.Email,
                UserName = domain.UserName,

                ContactInfo = domain.ContactInfo.ToOwned(),
                ProfileImage = domain.ProfileImage.ToOwned()
            };
    }

    public static void UpdateEntity(User domain, ApplicationUserEntity entity)
    {
        if (domain is null) throw new ArgumentNullException(nameof(domain));
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        entity.TenantId = domain.TenantId;

        entity.FullName = domain.FullName;
        entity.Department = domain.Department;

        entity.DateOfBirth = domain.DateOfBirth;

        // هذول من الدومين (rehydrated أو updated)
        entity.LastLoginAt = domain.LastLoginAt;
        entity.MustChangePassword = domain.MustChangePassword;

        // IdentityUser fields (Email/UserName) — خليهم متزامنين
        entity.Email = domain.Email;
        entity.UserName = domain.UserName;

        entity.ContactInfo = domain.ContactInfo.ToOwned();
        entity.ProfileImage = domain.ProfileImage.ToOwned();

        entity.UpdatedAt = domain.UpdatedAt ?? DateTime.UtcNow;
    }
}