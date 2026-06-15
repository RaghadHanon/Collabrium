using Application.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;
public sealed class AnnouncementsRepository
    : IAnnouncementsRepository
{
    private readonly AppDbContext _dbContext;

    public AnnouncementsRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        Announcement announcement,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(announcement);

        var entity = AnnouncementMapper.ToEntity(announcement);

        await _dbContext.Announcements.AddAsync(entity, cancellationToken);
    }

    public async Task<Announcement?> GetByIdAsync(
        string tenantId,
        string announcementId,
        CancellationToken cancellationToken = default)
    {
        tenantId = tenantId?.Trim() ?? "";
        announcementId = announcementId?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(announcementId))
        {
            return null;
        }

        var entity = await _dbContext.Announcements
            .AsNoTracking()
            .Include(a => a.Audiences)
            .FirstOrDefaultAsync(
                a =>
                    a.TenantId == tenantId &&
                    a.Id == announcementId,
                cancellationToken);

        return entity is null
            ? null
            : AnnouncementMapper.ToDomain(entity);
    }

    public async Task<bool> ExistsAsync(
        string tenantId,
        string announcementId,
        CancellationToken cancellationToken = default)
    {
        tenantId = tenantId?.Trim() ?? "";
        announcementId = announcementId?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(announcementId))
        {
            return false;
        }

        return await _dbContext.Announcements
            .AsNoTracking()
            .AnyAsync(
                a =>
                    a.TenantId == tenantId &&
                    a.Id == announcementId,
                cancellationToken);
    }

    public async Task UpdateAsync(
        Announcement announcement,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(announcement);

        var entity = await _dbContext.Announcements
            .Include(a => a.Audiences)
            .FirstOrDefaultAsync(
                a =>
                    a.TenantId == announcement.TenantId &&
                    a.Id == announcement.Id,
                cancellationToken);

        if (entity is null)
            throw new InvalidOperationException("Announcement not found.");

        AnnouncementMapper.UpdateEntity(entity, announcement);

        entity.Audiences.Clear();

        foreach (var audience in announcement.Audiences)
        {
            entity.Audiences.Add(
                AnnouncementAudienceMapper.ToEntity(audience));
        }
    }

    public async Task<bool> DeleteAsync(
        string tenantId,
        string announcementId,
        CancellationToken cancellationToken = default)
    {
        tenantId = tenantId?.Trim() ?? "";
        announcementId = announcementId?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(announcementId))
        {
            return false;
        }

        var entity = await _dbContext.Announcements
            .FirstOrDefaultAsync(
                a =>
                    a.TenantId == tenantId &&
                    a.Id == announcementId,
                cancellationToken);

        if (entity is null)
            return false;

        _dbContext.Announcements.Remove(entity);

        return true;
    }
    public async Task<List<Announcement>> GetAnnouncementsDueForNotificationAsync(
    DateTime now,
    CancellationToken ct = default)
    {
        var entities = await _dbContext.Announcements
            .AsNoTracking()
            .Include(a => a.Audiences)
            .Where(a =>
                !a.NotificationSent &&
                a.PublishAt <= now &&
                (a.ExpiresAt == null || a.ExpiresAt > now))
            .ToListAsync(ct);

        return entities
            .Select(AnnouncementMapper.ToDomain)
            .ToList();
    }
}