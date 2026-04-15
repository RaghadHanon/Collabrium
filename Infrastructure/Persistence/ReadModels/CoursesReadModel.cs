using Application.Common;
using Application.Features.Courses.DTOs;
using Application.Interfaces.ReadModels;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Enums;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.ReadModels
{
    public sealed class CoursesReadModel : ICoursesReadModel
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;

        public CoursesReadModel(AppDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<CourseDto?> GetByIdAsync(
            string tenantId,
            string courseId,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(courseId))
                return null;

            var tId = tenantId.Trim();
            var cId = courseId.Trim();

            return await _dbContext.Courses
                .AsNoTracking()
                .Where(c => c.TenantId == tId && c.Id == cId)
                .ProjectTo<CourseDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<PagedList<CourseListItemDto, DateTime?>> GetCoursesAsync(
            CoursesSearchAndFilterParametersDto parameters,
            string? currentUserId,
            CancellationToken cancellationToken = default)
        {
            parameters ??= new CoursesSearchAndFilterParametersDto();

            var pageSize = parameters.PageSize <= 0 ? 10 : parameters.PageSize;

            IQueryable<CourseEntity> query = _dbContext.Courses.AsNoTracking();

            query = ApplyFilters(query, parameters);

            if (parameters.Cursor.HasValue)
                query = query.Where(c => c.CreatedAt < parameters.Cursor.Value);

            var items = await query
                .OrderByDescending(c => c.CourseRatings.Any()
                    ? c.CourseRatings.Average(r => (double?)r.Rating) ?? 0
                    : 0)
                .ThenByDescending(c => c.CreatedAt)
                .Select(c => new CourseListItemDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    Url = c.Url,
                    Provider = c.Provider,
                    InstructorName = c.InstructorName,
                    Level = c.Level,
                    Language = c.Language,
                    EstimatedDurationInHours = c.EstimatedDurationInHours,

                    ThumbnailImageUrl = c.ThumbnailImage != null ? c.ThumbnailImage.Url : null,
                    ThumbnailImagePublicId = c.ThumbnailImage != null ? c.ThumbnailImage.PublicId : null,

                    TagIds = c.CourseTags.Select(t => t.TagId).ToList(),

                    RatingsCount = c.CourseRatings.Count,
                    AverageRating = c.CourseRatings.Any()
                        ? c.CourseRatings.Average(r => (double?)r.Rating) ?? 0
                        : 0,

                    IsSaved = !string.IsNullOrWhiteSpace(currentUserId) &&
                              c.UserCourses.Any(uc =>
                                  uc.UserId == currentUserId &&
                                  uc.IsSaved),

                    Status = !string.IsNullOrWhiteSpace(currentUserId)
                        ? c.UserCourses
                            .Where(uc => uc.UserId == currentUserId)
                            .Select(uc => (UserCourseStatus?)uc.Status)
                            .FirstOrDefault()
                        : null,



                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .Take(pageSize + 1)
                .ToListAsync(cancellationToken);

            DateTime? nextCursor = null;

            if (items.Count > pageSize)
            {
                nextCursor = items[^1].CreatedAt;
                items.RemoveAt(items.Count - 1);
            }

            return new PagedList<CourseListItemDto, DateTime?>
            {
                Items = items,
                NextCursor = nextCursor
            };
        }

        public async Task<CourseDetailsDto?> GetDetailsByIdAsync(
            string tenantId,
            string courseId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(courseId))
                return null;

            var tId = tenantId.Trim();
            var cId = courseId.Trim();

            return await _dbContext.Courses
                .AsNoTracking()
                .Where(c => c.TenantId == tId && c.Id == cId)
                .ProjectTo<CourseDetailsDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<CourseTagDto>> GetTagsAsync(
            string tenantId,
            string courseId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(courseId))
                return new();

            var tId = tenantId.Trim();
            var cId = courseId.Trim();

            return await _dbContext.Courses
                .AsNoTracking()
                .Where(c => c.TenantId == tId && c.Id == cId)
                .SelectMany(c => c.CourseTags)
                .ProjectTo<CourseTagDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }

        private static IQueryable<CourseEntity> ApplyFilters(
            IQueryable<CourseEntity> query,
            CoursesSearchAndFilterParametersDto parameters)
        {
            if (!string.IsNullOrWhiteSpace(parameters.TenantId))
            {
                var tenantId = parameters.TenantId.Trim();
                query = query.Where(c => c.TenantId == tenantId);
            }

            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                var term = parameters.Search.Trim();

                query = query.Where(c =>
                    c.Title.Contains(term) ||
                    (c.Description != null && c.Description.Contains(term)) ||
                    c.Provider.Contains(term) ||
                    (c.InstructorName != null && c.InstructorName.Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(parameters.Provider))
            {
                var provider = parameters.Provider.Trim();
                query = query.Where(c => c.Provider.Contains(provider));
            }

            if (!string.IsNullOrWhiteSpace(parameters.InstructorName))
            {
                var instructor = parameters.InstructorName.Trim();
                query = query.Where(c =>
                    c.InstructorName != null &&
                    c.InstructorName.Contains(instructor));
            }

            if (parameters.Level.HasValue)
                query = query.Where(c => c.Level == parameters.Level.Value);

            if (parameters.Language.HasValue)
                query = query.Where(c => c.Language == parameters.Language.Value);

            if (parameters.MinEstimatedDurationInHours.HasValue)
            {
                query = query.Where(c =>
                    c.EstimatedDurationInHours.HasValue &&
                    c.EstimatedDurationInHours.Value >= parameters.MinEstimatedDurationInHours.Value);
            }

            if (parameters.MaxEstimatedDurationInHours.HasValue)
            {
                query = query.Where(c =>
                    c.EstimatedDurationInHours.HasValue &&
                    c.EstimatedDurationInHours.Value <= parameters.MaxEstimatedDurationInHours.Value);
            }

            if (parameters.HasThumbnail.HasValue)
            {
                if (parameters.HasThumbnail.Value)
                    query = query.Where(c => c.ThumbnailImage != null);
                else
                    query = query.Where(c => c.ThumbnailImage == null);
            }

            if (parameters.TagIds is { Count: > 0 })
            {
                var tagIds = parameters.TagIds
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim())
                    .Distinct()
                    .ToList();

                if (tagIds.Count > 0)
                {
                    query = query.Where(c =>
                        c.CourseTags.Any(ct => tagIds.Contains(ct.TagId)));
                }
            }

            if (parameters.CreatedAfter.HasValue)
                query = query.Where(c => c.CreatedAt >= parameters.CreatedAfter.Value);

            if (parameters.CreatedBefore.HasValue)
                query = query.Where(c => c.CreatedAt <= parameters.CreatedBefore.Value);

            if (parameters.UpdatedAfter.HasValue)
            {
                query = query.Where(c =>
                    c.UpdatedAt != null &&
                    c.UpdatedAt >= parameters.UpdatedAfter.Value);
            }

            if (parameters.UpdatedBefore.HasValue)
            {
                query = query.Where(c =>
                    c.UpdatedAt != null &&
                    c.UpdatedAt <= parameters.UpdatedBefore.Value);
            }

            return query;
        }

        public async Task<CourseStatsDto?> GetStatsByIdAsync(
            string tenantId,
            string courseId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(courseId))
                return null;

            var tId = tenantId.Trim();
            var cId = courseId.Trim();

            var exists = await _dbContext.Courses
                .AsNoTracking()
                .AnyAsync(c => c.TenantId == tId && c.Id == cId, cancellationToken);

            if (!exists)
                return null;

            var userCoursesQuery = _dbContext.UserCourses
                .AsNoTracking()
                .Where(uc => uc.TenantId == tId && uc.CourseId == cId);

            var savedCount = await userCoursesQuery
                .CountAsync(uc => uc.IsSaved, cancellationToken);

            var assignedCount = await userCoursesQuery
                .CountAsync(uc => uc.IsAssigned, cancellationToken);

            var inProgressCount = await userCoursesQuery
                .CountAsync(uc => uc.Status == UserCourseStatus.InProgress, cancellationToken);

            var completedCount = await userCoursesQuery
                .CountAsync(uc => uc.Status == UserCourseStatus.Completed, cancellationToken);
            return new CourseStatsDto
            {
                SavedCount = savedCount,
                AssignedCount = assignedCount,
                InProgressCount = inProgressCount,
                CompletedCount = completedCount
            };
        }
    }
}
