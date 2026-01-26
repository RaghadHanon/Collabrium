using Application.Common;
using Application.Common.DTOs;
using Application.Features.Tenants.DTOs;
using Application.Features.Users.DTOs;
using Application.Interfaces.ReadModels;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.ReadModels;

public sealed class UsersReadModel : IUsersReadModel
{
    private readonly AppDbContext _dbContext;

    public UsersReadModel(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<UserWithTenantDto?> GetUserWithTenantAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Task.FromResult<UserWithTenantDto?>(null);

        var id = userId.Trim();

        return _dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new UserWithTenantDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email ?? "",
                Department = (Department) u.Department!,

                Tenant = u.Tenant == null ? null : new TenantDto
                {
                    Id = u.Tenant.Id,
                    CreatedAt = u.Tenant.CreatedAt,
                    Name = u.Tenant.Name,
                    Domain = u.Tenant.Domain,
                    Description = u.Tenant.Description,
                    IsActive = u.Tenant.IsActive,

                    ProfileImageUrl = u.Tenant.ProfileImage != null ? u.Tenant.ProfileImage.Url : null,
                    Country = u.Tenant.Country,
                    WebsiteUrl = u.Tenant.WebsiteUrl,

                    ContactInfo = new ContactDetailsDto
                    {
                        Email = u.Tenant.ContactInfo.Email,
                        PhoneNumber = u.Tenant.ContactInfo.PhoneNumber,
                        Socials = new SocialLinksDto
                        {
                            LinkedIn = u.Tenant.ContactInfo.Socials.LinkedIn,
                            Facebook = u.Tenant.ContactInfo.Socials.Facebook,
                            Instagram = u.Tenant.ContactInfo.Socials.Instagram,
                            YouTube = u.Tenant.ContactInfo.Socials.YouTube
                        }
                    }
                },

                ContactInfo = new ContactDetailsDto
                {
                    Email = u.ContactInfo.Email,
                    PhoneNumber = u.ContactInfo.PhoneNumber,
                    Socials = new SocialLinksDto
                    {
                        LinkedIn = u.ContactInfo.Socials.LinkedIn,
                        Facebook = u.ContactInfo.Socials.Facebook,
                        Instagram = u.ContactInfo.Socials.Instagram,
                        YouTube = u.ContactInfo.Socials.YouTube
                    }
                }
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedList<UserDto, DateTime?>> GetUsersAsync(
    UsersSearchAndFilterParametersDto parameters,
    CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Users
            .AsNoTracking()
            .AsQueryable();

        // ---------------- Filters ----------------
        if (!string.IsNullOrWhiteSpace(parameters.TenantId))
            query = query.Where(u => u.TenantId == parameters.TenantId);

        if (!string.IsNullOrWhiteSpace(parameters.NameContains))
            query = query.Where(u => u.FullName.Contains(parameters.NameContains));

        if (parameters.Department.HasValue)
            query = query.Where(u => u.Department == parameters.Department.Value);

        if (parameters.CreatedAfter.HasValue)
            query = query.Where(u => u.CreatedAt >= parameters.CreatedAfter.Value);

        if (parameters.CreatedBefore.HasValue)
            query = query.Where(u => u.CreatedAt <= parameters.CreatedBefore.Value);

        if (parameters.LastLoginAfter.HasValue)
            query = query.Where(u => u.LastLoginAt >= parameters.LastLoginAfter.Value);

        if (parameters.DateOfBirth.HasValue)
            query = query.Where(u => u.DateOfBirth >= parameters.DateOfBirth.Value);

        if (!string.IsNullOrWhiteSpace(parameters.PhoneNumber))
            query = query.Where(u => u.PhoneNumber == parameters.PhoneNumber);

        // Cursor pagination
        if (parameters.Cursor.HasValue)
            query = query.Where(u => u.CreatedAt > parameters.Cursor.Value);

        // ---------------- Query execution ----------------
        var entities = await query
            .OrderBy(u => u.CreatedAt)
            .Take(parameters.PageSize + 1)
            .Select(u => new UserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email ?? "",
                Department = (Department) u.Department!,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt,
                ContactInfo = new ContactDetailsDto
                {
                    Email = u.ContactInfo.Email,
                    PhoneNumber = u.ContactInfo.PhoneNumber,
                    Socials = new SocialLinksDto
                    {
                        LinkedIn = u.ContactInfo.Socials.LinkedIn,
                        Facebook = u.ContactInfo.Socials.Facebook,
                        Instagram = u.ContactInfo.Socials.Instagram,
                        YouTube = u.ContactInfo.Socials.YouTube
                    }
                }
            })
            .ToListAsync(cancellationToken);

        // ---------------- Cursor handling ----------------
        DateTime? nextCursor = null;
        if (entities.Count > parameters.PageSize)
        {
            nextCursor = entities.Last().CreatedAt;
            entities.RemoveAt(entities.Count - 1);
        }

        return new PagedList<UserDto, DateTime?>
        {
            Items = entities,
            NextCursor = nextCursor
        };
    }
}