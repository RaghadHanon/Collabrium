using Application.Common;
using Application.Common.DTOs;
using Application.Features.Tenants.DTOs;
using Application.Interfaces.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.ReadModels;

public sealed class TenantsReadModel : ITenantsReadModel
{
    private readonly AppDbContext _dbContext;

    public TenantsReadModel(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<TenantDto?> GetByIdAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId)) return Task.FromResult<TenantDto?>(null);

        var id = tenantId.Trim();

        return _dbContext.Tenants
            .AsNoTracking()
            .Where(t => t.Id == id)
            .Select(MapTenantDto())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<TenantDto?> GetByDomainAsync(string domain, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(domain)) return Task.FromResult<TenantDto?>(null);

        var normalized = domain.Trim().ToLowerInvariant();

        return _dbContext.Tenants
            .AsNoTracking()
            .Where(t => t.Domain == normalized)
            .Select(MapTenantDto())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<TenantWithAdminDto?> GetWithAdminAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId)) return Task.FromResult<TenantWithAdminDto?>(null);

        var id = tenantId.Trim();

        return _dbContext.Tenants
            .AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new TenantWithAdminDto
            {
                Id = t.Id,
                CreatedAt = t.CreatedAt,
                Name = t.Name,
                Domain = t.Domain,
                Description = t.Description,
                IsActive = t.IsActive,
                ProfileImageUrl = t.ProfileImage != null ? t.ProfileImage.Url : null,
                Country = t.Country,
                WebsiteUrl = t.WebsiteUrl,

                ContactInfo = new ContactDetailsDto
                {
                    Email = t.ContactInfo.Email,
                    PhoneNumber = t.ContactInfo.PhoneNumber,
                    Socials = new SocialLinksDto
                    {
                        LinkedIn = t.ContactInfo.Socials.LinkedIn,
                        Facebook = t.ContactInfo.Socials.Facebook,
                        Instagram = t.ContactInfo.Socials.Instagram,
                        YouTube = t.ContactInfo.Socials.YouTube
                    }
                },

                TenantAdmin = t.TenantAdminId == null ? null : new TenantAdminDto
                {
                    Id = t.TenantAdminId,
                    FullName = t.Users
                        .Where(u => u.Id == t.TenantAdminId)
                        .Select(u => u.FullName)
                        .FirstOrDefault() ?? "",
                    PhoneNumber = t.Users
                        .Where(u => u.Id == t.TenantAdminId)
                        .Select(u => u.PhoneNumber ?? "")
                        .FirstOrDefault() ?? "",
                    Email = t.Users
                        .Where(u => u.Id == t.TenantAdminId)
                        .Select(u => u.Email ?? "")
                        .FirstOrDefault() ?? "",
                    Department = t.Users
                        .Where(u => u.Id == t.TenantAdminId)
                        .Select(u => u.Department)
                        .FirstOrDefault()
                }
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedList<TenantDto, DateTime?>> GetTenantsAsync(
        TenantsSearchAndFilterParametersDto parameters,
        CancellationToken cancellationToken = default)
    {
        parameters ??= new TenantsSearchAndFilterParametersDto();

        var query = _dbContext.Tenants
            .AsNoTracking()
            .AsQueryable();

        query = ApplyFilters(query, parameters);

        if (parameters.Cursor.HasValue)
            query = query.Where(t => t.CreatedAt >= parameters.Cursor.Value);

        var items = await query
            .OrderBy(t => t.CreatedAt)
            .Select(MapTenantDto())
            .Take(parameters.PageSize + 1)
            .ToListAsync(cancellationToken);

        DateTime? nextCursor = null;
        if (items.Count > parameters.PageSize)
        {
            nextCursor = items.Last().CreatedAt;
            items.RemoveAt(items.Count - 1);
        }

        return new PagedList<TenantDto, DateTime?>
        {
            Items = items,
            NextCursor = nextCursor
        };
    }

    private static IQueryable<Entities.TenantEntity> ApplyFilters(
        IQueryable<Entities.TenantEntity> query,
        TenantsSearchAndFilterParametersDto parameters)
    {
        if (!string.IsNullOrWhiteSpace(parameters.Country))
        {
            var country = parameters.Country.Trim().ToLowerInvariant();
            query = query.Where(t => t.Country != null && t.Country.ToLower() == country);
        }

        if (!string.IsNullOrWhiteSpace(parameters.DescriptionContains))
        {
            var term = parameters.DescriptionContains.Trim();
            query = query.Where(t => t.Description != null && t.Description.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(parameters.NameContains))
        {
            var term = parameters.NameContains.Trim();
            query = query.Where(t => t.Name.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(parameters.DomainContains))
        {
            var term = parameters.DomainContains.Trim().ToLowerInvariant();
            query = query.Where(t => t.Domain.Contains(term));
        }

        if (parameters.IsActive.HasValue)
            query = query.Where(t => t.IsActive == parameters.IsActive.Value);

        if (parameters.CreatedAfter.HasValue)
            query = query.Where(t => t.CreatedAt >= parameters.CreatedAfter.Value);

        if (parameters.CreatedBefore.HasValue)
            query = query.Where(t => t.CreatedAt <= parameters.CreatedBefore.Value);

        if (!string.IsNullOrWhiteSpace(parameters.PhoneNumber))
        {
            var phone = parameters.PhoneNumber.Trim();
            query = query.Where(t => t.ContactInfo.PhoneNumber == phone);
        }

        return query;
    }

    private static System.Linq.Expressions.Expression<Func<Entities.TenantEntity, TenantDto>> MapTenantDto()
    {
        return t => new TenantDto
        {
            Id = t.Id,
            CreatedAt = t.CreatedAt,
            Name = t.Name,
            Domain = t.Domain,
            Description = t.Description,
            IsActive = t.IsActive,
            ProfileImageUrl = t.ProfileImage != null ? t.ProfileImage.Url : null,
            Country = t.Country,
            WebsiteUrl = t.WebsiteUrl,
            ContactInfo = new ContactDetailsDto
            {
                Email = t.ContactInfo.Email,
                PhoneNumber = t.ContactInfo.PhoneNumber,
                Socials = new SocialLinksDto
                {
                    LinkedIn = t.ContactInfo.Socials.LinkedIn,
                    Facebook = t.ContactInfo.Socials.Facebook,
                    Instagram = t.ContactInfo.Socials.Instagram,
                    YouTube = t.ContactInfo.Socials.YouTube
                }
            }
        };
    }
}