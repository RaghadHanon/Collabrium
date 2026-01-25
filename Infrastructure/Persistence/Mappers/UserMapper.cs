using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers;

public static class UserMapper
{
    public static User ToDomain(ApplicationUserEntity entity)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        var domain = new User(
            id: entity.Id,
            fullName: entity.FullName,
            dateOfBirth: entity.DateOfBirth,
            email: entity.Email ?? string.Empty,
            userName: entity.UserName ?? string.Empty,
            tenantId: entity.TenantId,
            department: entity.Department
        );

        if (entity.MustChangePassword)
            domain.RequirePasswordChange();

        if (entity.UserContactInfo != null)
            domain.SetContactInfo(UserContactInfoMapper.ToDomain(entity.UserContactInfo));

        if (entity.ProfileImage != null)
            domain.SetProfileImage(UserImageMapper.ToDomain(entity.ProfileImage));

        return domain;
    }

    public static ApplicationUserEntity ToEntity(User domain)
    {
        if (domain is null) throw new ArgumentNullException(nameof(domain));

        var entity = new ApplicationUserEntity
        {
            Id = domain.Id,
            FullName = domain.FullName,
            DateOfBirth = domain.DateOfBirth,
            TenantId = domain.TenantId,
            Department = domain.Department,
            MustChangePassword = domain.MustChangePassword,
            CreatedAt = domain.CreatedAt,
            Email = domain.Email,
            UserName = domain.UserName
        };

        if (domain.ContactInfo != null)
        {
            entity.UserContactInfo = UserContactInfoMapper.ToEntity(domain.ContactInfo);
            entity.UserContactInfo.UserId = entity.Id;
        }

        if (domain.ProfileImage != null)
        {
            entity.ProfileImage = UserImageMapper.ToEntity(domain.ProfileImage);
            entity.ProfileImage.ApplicationUserId = entity.Id;
        }

        return entity;
    }

    public static void UpdateEntity(User domain, ApplicationUserEntity entity)
    {
        if (domain is null) throw new ArgumentNullException(nameof(domain));
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        entity.FullName = domain.FullName;
        entity.DateOfBirth = domain.DateOfBirth;
        entity.TenantId = domain.TenantId;
        entity.Department = domain.Department;
        entity.MustChangePassword = domain.MustChangePassword;
        entity.Email = domain.Email;
        entity.UserName = domain.UserName;

        if (domain.ContactInfo == null)
        {
            entity.UserContactInfo = null;
        }
        else
        {
            entity.UserContactInfo ??= new UserContactInfoEntity();
            UserContactInfoMapper.UpdateEntity(domain.ContactInfo, entity.UserContactInfo);
            entity.UserContactInfo.UserId = entity.Id;
        }

        if (domain.ProfileImage == null)
        {
            entity.ProfileImage = null;
        }
        else
        {
            entity.ProfileImage ??= new UserImageEntity();
            UserImageMapper.UpdateEntity(domain.ProfileImage, entity.ProfileImage);
            entity.ProfileImage.ApplicationUserId = entity.Id;
        }
    }
}
