using Application.Common;
using Application.Features.UserCourses.DTOs;
using Application.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class UserCoursesRepository : IUserCoursesRepository
{
    private readonly AppDbContext _dbContext;

    public UserCoursesRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task AddAsync(UserCourse userCourse, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(userCourse);

        var entity = UserCourseMapper.ToEntity(userCourse);
        await _dbContext.UserCourses.AddAsync(entity, ct);
    }

    public async Task UpdateAsync(UserCourse userCourse, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(userCourse);

        var entity = await _dbContext.UserCourses
            .FirstOrDefaultAsync(x =>
                x.Id == userCourse.Id &&
                x.TenantId == userCourse.TenantId, ct);

        if (entity is null)
            throw new InvalidOperationException("User course not found.");

        UserCourseMapper.UpdateEntity(entity, userCourse);
    }

    public async Task<UserCourse?> GetByIdAsync(
        string tenantId,
        string userCourseId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(userCourseId))
            return null;

        var tId = tenantId.Trim();
        var ucId = userCourseId.Trim();

        var entity = await _dbContext.UserCourses
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.TenantId == tId &&
                x.Id == ucId, ct);

        return entity is null ? null : UserCourseMapper.ToDomain(entity);
    }

    public async Task<UserCourse?> GetByUserAndCourseAsync(
        string tenantId,
        string userId,
        string courseId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(userId) ||
            string.IsNullOrWhiteSpace(courseId))
            return null;

        var tId = tenantId.Trim();
        var uId = userId.Trim();
        var cId = courseId.Trim();

        var entity = await _dbContext.UserCourses
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.TenantId == tId &&
                x.UserId == uId &&
                x.CourseId == cId, ct);

        return entity is null ? null : UserCourseMapper.ToDomain(entity);
    }

    public Task<bool> ExistsAsync(
        string tenantId,
        string userId,
        string courseId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(userId) ||
            string.IsNullOrWhiteSpace(courseId))
            return Task.FromResult(false);

        var tId = tenantId.Trim();
        var uId = userId.Trim();
        var cId = courseId.Trim();

        return _dbContext.UserCourses
            .AsNoTracking()
            .AnyAsync(x =>
                x.TenantId == tId &&
                x.UserId == uId &&
                x.CourseId == cId, ct);
    }
    public async Task<bool> DeleteAsync(
       string tenantId,
       string userCourseId,
       CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(userCourseId))
            return false;

        var tId = tenantId.Trim();
        var ucId = userCourseId.Trim();

        var entity = await _dbContext.UserCourses
            .FirstOrDefaultAsync(x => x.TenantId == tId && x.Id == ucId, cancellationToken);

        if (entity is null)
            return false;

        _dbContext.UserCourses.Remove(entity);
        return true;
    }

    public async Task<bool> DeleteByUserAndCourseAsync(
        string tenantId,
        string userId,
        string courseId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(userId) ||
            string.IsNullOrWhiteSpace(courseId))
            return false;

        var tId = tenantId.Trim();
        var uId = userId.Trim();
        var cId = courseId.Trim();

        var entity = await _dbContext.UserCourses
            .FirstOrDefaultAsync(x =>
                x.TenantId == tId &&
                x.UserId == uId &&
                x.CourseId == cId,
                cancellationToken);

        if (entity is null)
            return false;

        _dbContext.UserCourses.Remove(entity);
        return true;
    }

    public async Task<PagedList<UserCourse, DateTime?>> GetPagedAsync(
         UserCoursesSearchAndFilterParametersDto parameters,
         CancellationToken cancellationToken = default)
    {
        parameters ??= new UserCoursesSearchAndFilterParametersDto();

        var pageSize = parameters.PageSize <= 0 ? 10 : parameters.PageSize;

        var query = _dbContext.UserCourses.AsNoTracking();
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

        return new PagedList<UserCourse, DateTime?>
        {
            Items = entities.Select(UserCourseMapper.ToDomain).ToList(),
            NextCursor = nextCursor
        };
    }

    private static IQueryable<UserCourseEntity> ApplyFilters(
        IQueryable<UserCourseEntity> query,
        UserCoursesSearchAndFilterParametersDto p)
    {
        if (!string.IsNullOrWhiteSpace(p.TenantId))
        {
            var tId = p.TenantId.Trim();
            query = query.Where(x => x.TenantId == tId);
        }

        if (!string.IsNullOrWhiteSpace(p.UserId))
        {
            var uId = p.UserId.Trim();
            query = query.Where(x => x.UserId == uId);
        }

        if (!string.IsNullOrWhiteSpace(p.CourseId))
        {
            var cId = p.CourseId.Trim();
            query = query.Where(x => x.CourseId == cId);
        }

        if (p.Status.HasValue)
            query = query.Where(x => x.Status == p.Status.Value);

        if (p.IsSaved.HasValue)
            query = query.Where(x => x.IsSaved == p.IsSaved.Value);

        if (p.IsAssigned.HasValue)
            query = query.Where(x => x.IsAssigned == p.IsAssigned.Value);

        if (p.StartedAfter.HasValue)
            query = query.Where(x => x.StartedAt != null && x.StartedAt >= p.StartedAfter.Value);

        if (p.StartedBefore.HasValue)
            query = query.Where(x => x.StartedAt != null && x.StartedAt <= p.StartedBefore.Value);

        if (p.CompletedAfter.HasValue)
            query = query.Where(x => x.CompletedAt != null && x.CompletedAt >= p.CompletedAfter.Value);

        if (p.CompletedBefore.HasValue)
            query = query.Where(x => x.CompletedAt != null && x.CompletedAt <= p.CompletedBefore.Value);

        if (p.AssignedAfter.HasValue)
            query = query.Where(x => x.AssignedAt != null && x.AssignedAt >= p.AssignedAfter.Value);

        if (p.AssignedBefore.HasValue)
            query = query.Where(x => x.AssignedAt != null && x.AssignedAt <= p.AssignedBefore.Value);

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
}
