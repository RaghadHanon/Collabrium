using Application.Common;
using Application.Features.CourseAssignments.DTOs;
using Application.Interfaces.ReadModels;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.ReadModels
{
    public sealed class CourseAssignmentsReadModel : ICourseAssignmentsReadModel
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;

        public CourseAssignmentsReadModel(AppDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<PagedList<CourseAssignmentDto, DateTime?>> GetAssignmentsAsync(
          CourseAssignmentsSearchParametersDto parameters,
          CancellationToken ct = default)
        {
            parameters ??= new CourseAssignmentsSearchParametersDto();

            var pageSize = parameters.PageSize <= 0 ? 10 : parameters.PageSize;

            IQueryable<CourseAssignmentEntity> query = _dbContext.CourseAssignments.AsNoTracking();

            query = ApplyFilters(query, parameters);

            if (parameters.Cursor.HasValue)
                query = query.Where(x => x.CreatedAt < parameters.Cursor.Value);

            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .ProjectTo<CourseAssignmentDto>(_mapper.ConfigurationProvider)
                .Take(pageSize + 1)
                .ToListAsync(ct);

            DateTime? nextCursor = null;

            if (items.Count > pageSize)
            {
                nextCursor = items[^1].CreatedAt;
                items.RemoveAt(items.Count - 1);
            }

            return new PagedList<CourseAssignmentDto, DateTime?>
            {
                Items = items,
                NextCursor = nextCursor
            };
        }


        public async Task<List<CourseAssignmentDto>> GetByCourseAsync(
            string tenantId,
            string courseId,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(courseId))
                return new List<CourseAssignmentDto>();

            var tId = tenantId.Trim();
            var cId = courseId.Trim();

            return await _dbContext.CourseAssignments
                .AsNoTracking()
                .Where(x => x.TenantId == tId && x.CourseId == cId)
                .OrderByDescending(x => x.CreatedAt)
                .ProjectTo<CourseAssignmentDto>(_mapper.ConfigurationProvider)
                .ToListAsync(ct);
        }

        public Task<CourseAssignmentDto?> GetByIdAsync(string tenantId, string assignmentId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(assignmentId))
                return Task.FromResult<CourseAssignmentDto?>(null);
            tenantId = tenantId.Trim();
            assignmentId = assignmentId.Trim();
            return _dbContext.CourseAssignments
                .AsNoTracking()
                .Where(a => a.TenantId == tenantId && a.Id == assignmentId)
                .ProjectTo<CourseAssignmentDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<List<CourseAssignmentDto>> GetByUserAsync(
            string tenantId,
            string userId,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(userId))
                return new List<CourseAssignmentDto>();

            var tId = tenantId.Trim();
            var uId = userId.Trim();

            return await _dbContext.CourseAssignments
                .AsNoTracking()
                .Where(x => x.TenantId == tId && x.AssignedToUserId == uId)
                .OrderByDescending(x => x.CreatedAt)
                .ProjectTo<CourseAssignmentDto>(_mapper.ConfigurationProvider)
                .ToListAsync(ct);
        }
        private static IQueryable<CourseAssignmentEntity> ApplyFilters(
          IQueryable<CourseAssignmentEntity> query,
          CourseAssignmentsSearchParametersDto p)
        {
            if (!string.IsNullOrWhiteSpace(p.TenantId))
            {
                var tenantId = p.TenantId.Trim();
                query = query.Where(x => x.TenantId == tenantId);
            }

            if (!string.IsNullOrWhiteSpace(p.CourseId))
            {
                var courseId = p.CourseId.Trim();
                query = query.Where(x => x.CourseId == courseId);
            }

            if (!string.IsNullOrWhiteSpace(p.AssignedToUserId))
            {
                var userId = p.AssignedToUserId.Trim();
                query = query.Where(x => x.AssignedToUserId == userId);
            }

            if (!string.IsNullOrWhiteSpace(p.AssignedByUserId))
            {
                var userId = p.AssignedByUserId.Trim();
                query = query.Where(x => x.AssignedByUserId == userId);
            }

            if (p.Status.HasValue)
                query = query.Where(x => x.Status == p.Status.Value);

            if (p.AssignedAfter.HasValue)
                query = query.Where(x => x.AssignedAt >= p.AssignedAfter.Value);

            if (p.AssignedBefore.HasValue)
                query = query.Where(x => x.AssignedAt <= p.AssignedBefore.Value);

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
        public async Task<CourseAssignmentDetailsDto?> GetDetailsByIdAsync(
    string tenantId,
    string assignmentId,
    CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(assignmentId))
                return null;

            var tId = tenantId.Trim();
            var aId = assignmentId.Trim();

            return await _dbContext.CourseAssignments
                .AsNoTracking()
                .Where(a => a.TenantId == tId && a.Id == aId)
                .Select(a => new CourseAssignmentDetailsDto
                {
                    Id = a.Id,
                    TenantId = a.TenantId,
                    CourseId = a.CourseId,

                    CourseTitle = a.Course.Title,
                    CourseThumbnailUrl = a.Course.ThumbnailImage != null
                        ? a.Course.ThumbnailImage.Url
                        : null,

                    AssignedToUserId = a.AssignedToUserId,
                    AssignedToUserName = a.AssignedToUser.FullName,

                    AssignedByUserId = a.AssignedByUserId,
                    AssignedByUserName = a.AssignedByUser.FullName,

                    AssignedAt = a.AssignedAt,
                    Status = a.Status,

                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt
                })
                .FirstOrDefaultAsync(ct);
        }
    }
}
