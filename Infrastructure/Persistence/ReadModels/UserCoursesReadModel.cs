using Application.Common;
using Application.Features.UserCourses.DTOs;
using Application.Interfaces.ReadModels;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Enums;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.ReadModels;

public sealed class UserCoursesReadModel : IUserCoursesReadModel
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;

    public UserCoursesReadModel(AppDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public Task<UserCourseDto?> GetByIdAsync(
        string tenantId,
        string userCourseId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(userCourseId))
            return Task.FromResult<UserCourseDto?>(null);

        var tId = tenantId.Trim();
        var ucId = userCourseId.Trim();

        return _dbContext.UserCourses
            .AsNoTracking()
            .Where(x => x.TenantId == tId && x.Id == ucId)
            .ProjectTo<UserCourseDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(ct);
    }

    public Task<UserCourseDto?> GetByUserAndCourseAsync(
        string tenantId,
        string userId,
        string courseId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(userId) ||
            string.IsNullOrWhiteSpace(courseId))
            return Task.FromResult<UserCourseDto?>(null);

        var tId = tenantId.Trim();
        var uId = userId.Trim();
        var cId = courseId.Trim();

        return _dbContext.UserCourses
            .AsNoTracking()
            .Where(x =>
                x.TenantId == tId &&
                x.UserId == uId &&
                x.CourseId == cId)
            .ProjectTo<UserCourseDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(ct);
    }

    public Task<UserCourseDetailsDto?> GetDetailsByIdAsync(
        string tenantId,
        string userCourseId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(userCourseId))
            return Task.FromResult<UserCourseDetailsDto?>(null);

        var tId = tenantId.Trim();
        var ucId = userCourseId.Trim();

        return _dbContext.UserCourses
            .AsNoTracking()
            .Where(x => x.TenantId == tId && x.Id == ucId)
            .ProjectTo<UserCourseDetailsDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<UserCourseDto>> GetByUserAsync(
        string tenantId,
        string userId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(userId))
            return new List<UserCourseDto>();

        var tId = tenantId.Trim();
        var uId = userId.Trim();

        return await _dbContext.UserCourses
            .AsNoTracking()
            .Where(x => x.TenantId == tId && x.UserId == uId)
            .OrderByDescending(x => x.CreatedAt)
            .ProjectTo<UserCourseDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }

    public async Task<PagedList<UserCourseDto, DateTime?>> GetUserCoursesAsync(
        UserCoursesSearchAndFilterParametersDto parameters,
        CancellationToken ct = default)
    {
        parameters ??= new UserCoursesSearchAndFilterParametersDto();

        var pageSize = parameters.PageSize <= 0 ? 10 : parameters.PageSize;

        var query = _dbContext.UserCourses.AsNoTracking();

        query = ApplyFilters(query, parameters);

        if (parameters.Cursor.HasValue)
            query = query.Where(x => x.CreatedAt < parameters.Cursor.Value);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .ProjectTo<UserCourseDto>(_mapper.ConfigurationProvider)
            .Take(pageSize + 1)
            .ToListAsync(ct);

        DateTime? nextCursor = null;

        if (items.Count > pageSize)
        {
            nextCursor = items[^1].CreatedAt;
            items.RemoveAt(items.Count - 1);
        }

        return new PagedList<UserCourseDto, DateTime?>
        {
            Items = items,
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

        if (!string.IsNullOrWhiteSpace(p.Search))
        {
            var term = p.Search.Trim();

            query = query.Where(x =>
                x.Course.Title.Contains(term) ||
                x.Course.Description.Contains(term) ||
                x.Course.Provider.Contains(term) ||
                (x.Course.InstructorName != null && x.Course.InstructorName.Contains(term)));
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


    public async Task<PagedList<AssignedUserCourseDto, DateTime?>> GetAssignedByUserAsync(
    AssignedUserCoursesSearchAndFilterParametersDto parameters,
    CancellationToken cancellationToken = default)
    {
        parameters ??= new AssignedUserCoursesSearchAndFilterParametersDto();

        var pageSize = parameters.PageSize <= 0 ? 10 : parameters.PageSize;

        var query = _dbContext.UserCourses.AsNoTracking();

        query = ApplyAssignedByUserFilters(query, parameters);

        if (parameters.Cursor.HasValue)
            query = query.Where(x => x.CreatedAt < parameters.Cursor.Value);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .ProjectTo<AssignedUserCourseDto>(_mapper.ConfigurationProvider)
            .Take(pageSize + 1)
            .ToListAsync(cancellationToken);

        DateTime? nextCursor = null;

        if (items.Count > pageSize)
        {
            nextCursor = items[^1].CreatedAt;
            items.RemoveAt(items.Count - 1);
        }

        return new PagedList<AssignedUserCourseDto, DateTime?>
        {
            Items = items,
            NextCursor = nextCursor
        };
    }

    private static IQueryable<UserCourseEntity> ApplyAssignedByUserFilters(
IQueryable<UserCourseEntity> query,
AssignedUserCoursesSearchAndFilterParametersDto p)
    {
        if (!string.IsNullOrWhiteSpace(p.TenantId))
        {
            var tId = p.TenantId.Trim();
            query = query.Where(x => x.TenantId == tId);
        }

        if (!string.IsNullOrWhiteSpace(p.AssignedByUserId))
        {
            var aId = p.AssignedByUserId.Trim();
            query = query.Where(x => x.AssignedByUserId == aId);
        }

        if (!string.IsNullOrWhiteSpace(p.AssignedToUserId))
        {
            var uId = p.AssignedToUserId.Trim();
            query = query.Where(x => x.UserId == uId);
        }

        if (!string.IsNullOrWhiteSpace(p.CourseId))
        {
            var cId = p.CourseId.Trim();
            query = query.Where(x => x.CourseId == cId);
        }

        if (!string.IsNullOrWhiteSpace(p.Search))
        {
            var term = p.Search.Trim();
            query = query.Where(x =>
                x.User.FullName.Contains(term) ||
                x.Course.Title.Contains(term) ||
                x.Course.Description.Contains(term) ||
                x.Course.Provider.Contains(term) ||
                (x.Course.InstructorName != null && x.Course.InstructorName.Contains(term)));
        }

        if (p.Status.HasValue)
            query = query.Where(x => x.Status == p.Status.Value);

        if (p.IsSaved.HasValue)
            query = query.Where(x => x.IsSaved == p.IsSaved.Value);

        query = p.IsAssigned.HasValue ? query.Where(x => x.IsAssigned == p.IsAssigned.Value) : query.Where(x => x.IsAssigned);

        if (p.AssignedAfter.HasValue)
            query = query.Where(x => x.AssignedAt != null && x.AssignedAt >= p.AssignedAfter.Value);

        if (p.AssignedBefore.HasValue)
            query = query.Where(x => x.AssignedAt != null && x.AssignedAt <= p.AssignedBefore.Value);

        if (p.StartedAfter.HasValue)
            query = query.Where(x => x.StartedAt != null && x.StartedAt >= p.StartedAfter.Value);

        if (p.StartedBefore.HasValue)
            query = query.Where(x => x.StartedAt != null && x.StartedAt <= p.StartedBefore.Value);

        if (p.CompletedAfter.HasValue)
            query = query.Where(x => x.CompletedAt != null && x.CompletedAt >= p.CompletedAfter.Value);

        if (p.CompletedBefore.HasValue)
            query = query.Where(x => x.CompletedAt != null && x.CompletedAt <= p.CompletedBefore.Value);

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
    public async Task<MyLearningSummaryDto> GetMyLearningSummaryAsync(
        string tenantId,
        string userId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(userId))
            return new MyLearningSummaryDto();

        tenantId = tenantId.Trim();
        userId = userId.Trim();

        var query = _dbContext.UserCourses
             .AsNoTracking()
             .Where(uc => uc.TenantId == tenantId && uc.UserId == userId);

        var allCoursesCount = await query.CountAsync(ct);

        var savedCount = await query.CountAsync(
            uc => uc.IsSaved,
            ct);

        var assignedCount = await query.CountAsync(
            uc => uc.IsAssigned,
            ct);

        var inProgressCount = await query.CountAsync(
            uc => uc.Status == UserCourseStatus.InProgress,
           ct);

        var completedCount = await query.CountAsync(
            uc => uc.Status == UserCourseStatus.Completed,
            ct);

        return new MyLearningSummaryDto
        {
            AllCoursesCount = allCoursesCount,
            SavedCount = savedCount,
            AssignedToMeCount = assignedCount,
            InProgressCount = inProgressCount,
            CompletedCount = completedCount
        };
    }
    public async Task<AssignedByMeStatsDto> GetAssignedByMeStatsAsync(
        string tenantId,
        string currentUserId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(currentUserId))
            return new AssignedByMeStatsDto();
        var tId = tenantId.Trim();
        var cId = currentUserId.Trim();
        var query = _dbContext.UserCourses
            .AsNoTracking()
            .Where(uc => uc.TenantId == tId && uc.AssignedByUserId == cId);
        var totalAssigned = await query.CountAsync(cancellationToken);

        var notStarted = await query.CountAsync(uc => uc.Status == UserCourseStatus.NotStarted,
                          cancellationToken);
        var inProgress = await query.CountAsync(uc => uc.Status == UserCourseStatus.InProgress,
              cancellationToken);
        var completed = await query.CountAsync(uc => uc.Status == UserCourseStatus.Completed,
                cancellationToken);
        return new AssignedByMeStatsDto
        {
            TotalAssigned = totalAssigned,
            NotStartedCount = notStarted,
            InProgressCount = inProgress,
            CompletedCount = completed

        };
    }
}
