using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mapping;

public static class TenantMapper
{
    public static Tenant ToDomain(TenantEntity entity)
    {
        return entity is null
            ? throw new ArgumentNullException(nameof(entity))
            : Tenant.Rehydrate(
            id: entity.Id,
            createdAt: entity.CreatedAt,
            updatedAt: entity.UpdatedAt,
            name: entity.Name,
            domain: entity.Domain,
            isActive: entity.IsActive,
            description: entity.Description,
            country: entity.Country,
            websiteUrl: entity.WebsiteUrl,
            tenantAdminId: entity.TenantAdminId,
            contactInfo: entity.ContactDetails.ToDomain(),
            profileImage: entity.ProfileImage.ToDomain()
        );
    }

    public static TenantEntity ToEntity(Tenant domain)
    {
        return domain is null
            ? throw new ArgumentNullException(nameof(domain))
            : new TenantEntity
            {
                Id = domain.Id,
                CreatedAt = domain.CreatedAt,
                UpdatedAt = domain.UpdatedAt,

                Name = domain.Name,
                Domain = domain.Domain,
                IsActive = domain.IsActive,

                Description = domain.Description,
                Country = domain.Country,
                WebsiteUrl = domain.WebsiteUrl,
                TenantAdminId = domain.TenantAdminId,

                ContactDetails = domain.ContactInfo.ToOwned(),
                ProfileImage = domain.ProfileImage.ToOwned()
            };
    }

    public static void UpdateEntity(Tenant domain, TenantEntity entity)
    {
        if (domain is null) throw new ArgumentNullException(nameof(domain));
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        entity.Name = domain.Name;
        entity.Domain = domain.Domain;
        entity.IsActive = domain.IsActive;

        entity.Description = domain.Description;
        entity.Country = domain.Country;
        entity.WebsiteUrl = domain.WebsiteUrl;
        entity.TenantAdminId = domain.TenantAdminId;

        entity.ContactDetails = domain.ContactInfo.ToOwned();
        entity.ProfileImage = domain.ProfileImage.ToOwned();

        entity.UpdatedAt = domain.UpdatedAt ?? DateTime.UtcNow;
    }
}