using Application.Common;
using Application.Features.CourseRatings.DTOs;
using Application.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class CourseRatingsRepository : ICourseRatingsRepository
{
    private readonly AppDbContext _dbContext;

    public CourseRatingsRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task AddAsync(CourseRating courseRating, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(courseRating);

        var entity = CourseRatingMapper.ToEntity(courseRating);
        await _dbContext.CourseRatings.AddAsync(entity, cancellationToken);
    }

    public async Task UpdateAsync(CourseRating courseRating, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(courseRating);

        var entity = await _dbContext.CourseRatings
            .FirstOrDefaultAsync(
                x => x.Id == courseRating.Id && x.TenantId == courseRating.TenantId,
                cancellationToken);

        if (entity is null)
            throw new InvalidOperationException("Course rating not found.");

        CourseRatingMapper.UpdateEntity(entity, courseRating);
    }

    public async Task<bool> RemoveAsync(
        string tenantId,
        string ratingId,
        CancellationToken cancellationToken = default)
    {
        if (!TryNormalize(tenantId, ratingId, out var tId, out var rId))
            return false;

        var entity = await _dbContext.CourseRatings
            .FirstOrDefaultAsync(x => x.TenantId == tId && x.Id == rId, cancellationToken);

        if (entity is null)
            return false;

        _dbContext.CourseRatings.Remove(entity);
        return true;
    }

    public async Task<CourseRating?> GetByIdAsync(
        string tenantId,
        string ratingId,
        CancellationToken cancellationToken = default)
    {
        if (!TryNormalize(tenantId, ratingId, out var tId, out var rId))
            return null;

        var entity = await _dbContext.CourseRatings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tId && x.Id == rId, cancellationToken);

        return entity is null
            ? null
            : CourseRatingMapper.ToDomain(entity);
    }

    public async Task<CourseRating?> GetByUserAndCourseAsync(
        string tenantId,
        string userId,
        string courseId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(userId) ||
            string.IsNullOrWhiteSpace(courseId))
            return null;

        var tId = tenantId.Trim();
        var uId = userId.Trim();
        var cId = courseId.Trim();

        var entity = await _dbContext.CourseRatings
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == tId &&
                     x.UserId == uId &&
                     x.CourseId == cId,
                cancellationToken);

        return entity is null
            ? null
            : CourseRatingMapper.ToDomain(entity);
    }

    public async Task<bool> ExistsAsync(
        string tenantId,
        string ratingId,
        CancellationToken cancellationToken = default)
    {
        return !TryNormalize(tenantId, ratingId, out var tId, out var rId)
            ? false
            : await _dbContext.CourseRatings
            .AsNoTracking()
            .AnyAsync(x => x.TenantId == tId && x.Id == rId, cancellationToken);
    }

    public async Task<PagedList<CourseRating, DateTime?>> GetPagedAsync(
        CourseRatingsSearchAndFilterParametersDto parameters,
        CancellationToken cancellationToken = default)
    {
        parameters ??= new CourseRatingsSearchAndFilterParametersDto();

        var pageSize = parameters.PageSize <= 0 ? 10 : parameters.PageSize;

        var query = _dbContext.CourseRatings.AsNoTracking();

        query = ApplyFilters(query, parameters);

        if (parameters.Cursor.HasValue)
            query = query.Where(x => x.CreatedAt < parameters.Cursor.Value);

        var entities = await query
            .OrderByDescending(x => x.CreatedAt)
            .Take(pageSize + 1)
            .ToListAsync(cancellationToken);

        DateTime? nextCursor = null;

        if (entities.Count > pageSize)
        {
            nextCursor = entities[^1].CreatedAt;
            entities.RemoveAt(entities.Count - 1);
        }

        return new PagedList<CourseRating, DateTime?>
        {
            Items = entities.Select(CourseRatingMapper.ToDomain).ToList(),
            NextCursor = nextCursor
        };
    }

    private static IQueryable<CourseRatingEntity> ApplyFilters(
        IQueryable<CourseRatingEntity> query,
        CourseRatingsSearchAndFilterParametersDto p)
    {
        if (!string.IsNullOrWhiteSpace(p.TenantId))
        {
            var tId = p.TenantId.Trim();
            query = query.Where(x => x.TenantId == tId);
        }

        if (!string.IsNullOrWhiteSpace(p.CourseId))
        {
            var cId = p.CourseId.Trim();
            query = query.Where(x => x.CourseId == cId);
        }

        if (!string.IsNullOrWhiteSpace(p.UserId))
        {
            var uId = p.UserId.Trim();
            query = query.Where(x => x.UserId == uId);
        }

        if (p.MinRating.HasValue)
            query = query.Where(x => x.Rating >= p.MinRating.Value);

        if (p.MaxRating.HasValue)
            query = query.Where(x => x.Rating <= p.MaxRating.Value);

        if (p.CreatedAfter.HasValue)
            query = query.Where(x => x.CreatedAt >= p.CreatedAfter.Value);

        if (p.CreatedBefore.HasValue)
            query = query.Where(x => x.CreatedAt <= p.CreatedBefore.Value);

        if (p.UpdatedAfter.HasValue)
            query = query.Where(x => x.UpdatedAt != null && x.UpdatedAt >= p.UpdatedAfter.Value);

        if (p.UpdatedBefore.HasValue)
            query = query.Where(x => x.UpdatedAt != null && x.UpdatedAt <= p.UpdatedBefore.Value);

        return query;
    }

    private static bool TryNormalize(string? tenantId, string? ratingId, out string tId, out string rId)
    {
        tId = tenantId?.Trim() ?? string.Empty;
        rId = ratingId?.Trim() ?? string.Empty;

        return !string.IsNullOrWhiteSpace(tId) &&
               !string.IsNullOrWhiteSpace(rId);
    }
}
