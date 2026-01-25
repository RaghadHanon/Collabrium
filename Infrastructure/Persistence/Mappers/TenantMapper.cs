using Domain.Models;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.Mappers;

public static class TenantMapper
{
    public static Tenant ToDomain(TenantEntity entity)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        var domain = new Tenant(entity.Id, entity.Name, entity.Domain);

        if (!entity.IsActive) domain.Deactivate();
        else domain.Activate();

        domain.UpdateProfile(entity.Description, entity.Country, entity.WebsiteURL);

        if (!string.IsNullOrWhiteSpace(entity.TenantAdminId))
            domain.AssignAdmin(entity.TenantAdminId);

        if (entity.TenantContactInfo != null)
            domain.SetContactInfo(TenantContactInfoMapper.ToDomain(entity.TenantContactInfo));

        if (entity.ProfileImage != null)
            domain.SetProfileImage(TenantImageMapper.ToDomain(entity.ProfileImage));

        return domain;
    }

    public static TenantEntity ToEntity(Tenant domain)
    {
        if (domain is null) throw new ArgumentNullException(nameof(domain));

        var entity = new TenantEntity
        {
            Id = domain.Id,
            CreatedAt = domain.CreatedAt,
            Name = domain.Name,
            Domain = domain.Domain,
            Description = domain.Description,
            Country = domain.Country,
            IsActive = domain.IsActive,
            WebsiteURL = domain.WebsiteUrl,
            TenantAdminId = domain.TenantAdminId,

        };

        if (domain.ContactInfo != null)
        {
            entity.TenantContactInfo = TenantContactInfoMapper.ToEntity(domain.ContactInfo);
            entity.TenantContactInfo.TenantId = entity.Id;
        }

        if (domain.ProfileImage != null)
        {
            entity.ProfileImage = TenantImageMapper.ToEntity(domain.ProfileImage);
            entity.ProfileImage.TenantId = entity.Id;
        }

        return entity;
    }

    public static void UpdateEntity(Tenant domain, TenantEntity entity)
    {
        if (domain is null) throw new ArgumentNullException(nameof(domain));
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        entity.Name = domain.Name;
        entity.Domain = domain.Domain;
        entity.Description = domain.Description;
        entity.Country = domain.Country;
        entity.IsActive = domain.IsActive;
        entity.WebsiteURL = domain.WebsiteUrl;
        entity.TenantAdminId = domain.TenantAdminId;

        if (domain.ContactInfo == null)
        {
            entity.TenantContactInfo = null;
        }
        else
        {
            entity.TenantContactInfo ??= new TenantContactInfoEntity();
            TenantContactInfoMapper.UpdateEntity(domain.ContactInfo, entity.TenantContactInfo);
            entity.TenantContactInfo.TenantId = entity.Id;
        }

        if (domain.ProfileImage == null)
        {
            entity.ProfileImage = null;
        }
        else
        {
            entity.ProfileImage ??= new TenantImageEntity();
            TenantImageMapper.UpdateEntity(domain.ProfileImage, entity.ProfileImage);
            entity.ProfileImage.TenantId = entity.Id;
        }
    }
}