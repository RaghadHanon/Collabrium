using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers;

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
            jobTitle: entity.JobTitle,
            dateOfBirth: entity.DateOfBirth,
            email: entity.Email ?? string.Empty,
            userName: entity.UserName ?? string.Empty,
            tenantId: entity.TenantId,
            department: entity.Department,
            lastLoginAt: entity.LastLoginAt,
            mustChangePassword: entity.MustChangePassword,
            contactInfo: entity.ContactDetails.ToDomain(),
            profileImage: entity.ProfileImage.ToDomain()
        );
    }

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
                JobTitle = domain.JobTitle,
                Department = domain.Department,

                DateOfBirth = domain.DateOfBirth,
                LastLoginAt = domain.LastLoginAt,
                MustChangePassword = domain.MustChangePassword,

                Email = domain.Email,
                UserName = domain.UserName,

                ContactDetails = domain.ContactInfo.ToOwned(),
                ProfileImage = domain.ProfileImage.ToOwned()
            };
    }

    public static void UpdateEntity(User domain, ApplicationUserEntity entity)
    {
        if (domain is null) throw new ArgumentNullException(nameof(domain));
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        entity.TenantId = domain.TenantId;

        entity.FullName = domain.FullName;
        entity.JobTitle = domain.JobTitle;
        entity.Department = domain.Department;

        entity.DateOfBirth = domain.DateOfBirth;

        entity.LastLoginAt = domain.LastLoginAt;
        entity.MustChangePassword = domain.MustChangePassword;

        entity.Email = domain.Email;
        entity.UserName = domain.UserName;

        entity.ContactDetails = domain.ContactInfo.ToOwned();
        entity.ProfileImage = domain.ProfileImage.ToOwned();

        entity.UpdatedAt = domain.UpdatedAt ?? DateTime.UtcNow;
    }
}