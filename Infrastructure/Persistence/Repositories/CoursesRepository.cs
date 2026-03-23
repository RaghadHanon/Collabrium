using Application.Common;
using Application.Features.Courses.DTOs;
using Application.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public sealed class CoursesRepository : ICoursesRepository
    {
        private readonly AppDbContext _dbContext;

        public CoursesRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(Course course, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(course);
            var entity = CourseMapper.ToEntity(course);
            await _dbContext.Courses.AddAsync(entity, cancellationToken);
        }

        public async Task AddTagsAsync(
            string tenantId,
            string courseId,
            IReadOnlyCollection<string> tagIds,
            CancellationToken cancellationToken = default)
        {
            if (!TryNormalize(tenantId, courseId, out var tId, out var cId))
                throw new ArgumentNullException("tenantId/courseId");

            var ids = NormalizeIds(tagIds);
            if (ids.Count == 0) return;

            var entity = await _dbContext.Courses
                .Include(c => c.CourseTags)
                .FirstOrDefaultAsync(c => c.TenantId == tId && c.Id == cId, cancellationToken);

            if (entity is null)
                throw new InvalidOperationException("Course not found.");

            await EnsureTagsExistInTenantAsync(tId, ids, cancellationToken);

            // domain logic
            var domain = CourseMapper.ToDomain(entity);
            foreach (var id in ids)
                domain.AddTag(id);

            // db logic
            var existing = entity.CourseTags
                .Select(x => x.TagId)
                .ToHashSet(StringComparer.Ordinal);

            foreach (var id in ids)
            {
                if (existing.Contains(id)) continue;

                entity.CourseTags.Add(new CourseTagEntity
                {
                    CourseId = entity.Id,
                    TagId = id
                });
            }

            CourseMapper.UpdateEntity(entity, domain);
        }

        public async Task<bool> DeleteAsync(string tenantId, string courseId, CancellationToken cancellationToken = default)
        {
            if (!TryNormalize(tenantId, courseId, out var tId, out var cId))
                return false;

            var entity = await _dbContext.Courses
                .FirstOrDefaultAsync(c => c.TenantId == tId && c.Id == cId, cancellationToken);

            if (entity is null)
                return false;

            _dbContext.Courses.Remove(entity);
            return true;
        }
        public async Task<bool> ExistsAsync(string tenantId, string courseId, CancellationToken cancellationToken = default)
        {
            if (!TryNormalize(tenantId, courseId, out var tId, out var cId))
                return false;

            return await _dbContext.Courses
                .AsNoTracking()
                .AnyAsync(c =>
                    c.TenantId == tId &&
                    c.Id == cId,
                    cancellationToken);
        }

        public async Task<Course?> GetByIdAsync(string tenantId, string courseId, CancellationToken cancellationToken = default)
        {
            if (!TryNormalize(tenantId, courseId, out var tId, out var cId))
                return null;

            var entity = await _dbContext.Courses
                .AsNoTracking()
                .Include(c => c.CourseTags)
                .Include(c => c.ThumbnailImage)
                .FirstOrDefaultAsync(c => c.TenantId == tId && c.Id == cId, cancellationToken);

            return entity is null
                ? null
                : CourseMapper.ToDomain(entity);
        }
        public async Task<PagedList<Course, DateTime?>> GetPagedAsync(
                   CoursesSearchAndFilterParametersDto parameters,
                   CancellationToken cancellationToken = default)
        {
            parameters ??= new CoursesSearchAndFilterParametersDto();

            var pageSize = parameters.PageSize <= 0 ? 10 : parameters.PageSize;

            IQueryable<CourseEntity> query = _dbContext.Courses.AsNoTracking();

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
                    c.Description.Contains(term) ||
                    c.Provider.Contains(term) ||
                    (c.InstructorName != null && c.InstructorName.Contains(term)));
            }

            if (parameters.Level.HasValue)
                query = query.Where(c => c.Level == parameters.Level.Value);

            if (parameters.Language.HasValue)
                query = query.Where(c => c.Language == parameters.Language.Value);

            if (parameters.Cursor.HasValue)
                query = query.Where(c => c.CreatedAt < parameters.Cursor.Value);
            if (!string.IsNullOrWhiteSpace(parameters.Provider))
                query = query.Where(c => c.Provider.Contains(parameters.Provider));

            if (!string.IsNullOrWhiteSpace(parameters.InstructorName))
                query = query.Where(c =>
                    c.InstructorName != null &&
                    c.InstructorName.Contains(parameters.InstructorName));

            if (parameters.HasThumbnail.HasValue)
            {
                if (parameters.HasThumbnail.Value)
                    query = query.Where(c => c.ThumbnailImage != null);
                else
                    query = query.Where(c => c.ThumbnailImage == null);
            }

            if (parameters.TagIds is { Count: > 0 })
            {
                var ids = NormalizeIds(parameters.TagIds);

                if (ids.Count > 0)
                {
                    query = query.Where(c =>
                        c.CourseTags.Any(ct => ids.Contains(ct.TagId)));
                }
            }
            var entities = await query
                .OrderByDescending(c => c.CreatedAt)
                .Include(c => c.CourseTags)
                .Include(c => c.ThumbnailImage)
                .Take(pageSize + 1)
                .ToListAsync(cancellationToken);

            DateTime? nextCursor = null;

            if (entities.Count > pageSize)
            {
                nextCursor = entities[^1].CreatedAt;
                entities.RemoveAt(entities.Count - 1);
            }

            return new PagedList<Course, DateTime?>
            {
                Items = entities.Select(CourseMapper.ToDomain).ToList(),
                NextCursor = nextCursor
            };
        }


        public async Task RemoveTagsAsync(
           string tenantId,
           string courseId,
           IReadOnlyCollection<string> tagIds,
           CancellationToken cancellationToken = default)
        {
            if (!TryNormalize(tenantId, courseId, out var tId, out var cId))
                throw new ArgumentNullException("tenantId/courseId");

            var ids = NormalizeIds(tagIds);
            if (ids.Count == 0) return;

            var entity = await _dbContext.Courses
                .Include(c => c.CourseTags)
                .FirstOrDefaultAsync(c => c.TenantId == tId && c.Id == cId, cancellationToken);

            if (entity is null)
                throw new InvalidOperationException("Course not found.");

            var domain = CourseMapper.ToDomain(entity);
            foreach (var id in ids)
                domain.RemoveTag(id);

            var toRemove = entity.CourseTags
                .Where(x => ids.Contains(x.TagId))
                .ToList();

            foreach (var row in toRemove)
                entity.CourseTags.Remove(row);

            CourseMapper.UpdateEntity(entity, domain);
        }

        public async Task SetTagsAsync(
                    string tenantId,
                    string courseId,
                    IReadOnlyCollection<string> tagIds,
                    CancellationToken cancellationToken = default)
        {
            if (!TryNormalize(tenantId, courseId, out var tId, out var cId))
                throw new ArgumentNullException("tenantId/courseId");

            var ids = NormalizeIds(tagIds);

            var entity = await _dbContext.Courses
                .Include(c => c.CourseTags)
                .FirstOrDefaultAsync(c => c.TenantId == tId && c.Id == cId, cancellationToken);

            if (entity is null)
                throw new InvalidOperationException("Course not found.");

            if (ids.Count > 0)
                await EnsureTagsExistInTenantAsync(tId, ids, cancellationToken);

            var domain = CourseMapper.ToDomain(entity);
            domain.SetTags(ids);

            var desired = new HashSet<string>(ids);

            entity.CourseTags = entity.CourseTags
                .Where(x => desired.Contains(x.TagId))
                .ToList();

            var existing = entity.CourseTags
                .Select(x => x.TagId)
                .ToHashSet();

            foreach (var id in ids)
            {
                if (existing.Contains(id)) continue;

                entity.CourseTags.Add(new CourseTagEntity
                {
                    CourseId = entity.Id,
                    TagId = id
                });
            }

            CourseMapper.UpdateEntity(entity, domain);
        }


        public async Task UpdateAsync(Course course, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(course);

            var entity = await _dbContext.Courses
                .FirstOrDefaultAsync(c => c.Id == course.Id, cancellationToken);

            if (entity is null)
                throw new InvalidOperationException("Course not found.");

            CourseMapper.UpdateEntity(entity, course);
        }
        private static bool TryNormalize(string? tenantId, string? courseId, out string tId, out string cId)
        {
            tId = tenantId?.Trim() ?? "";
            cId = courseId?.Trim() ?? "";

            return !string.IsNullOrWhiteSpace(tId) &&
                   !string.IsNullOrWhiteSpace(cId);
        }

        private static List<string> NormalizeIds(IReadOnlyCollection<string>? ids)
        {
            if (ids is null || ids.Count == 0) return new();

            return ids
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct()
                .ToList();
        }

        private async Task EnsureTagsExistInTenantAsync(
            string tenantId,
            List<string> tagIds,
            CancellationToken ct)
        {
            var tags = await _dbContext.Tags
                .AsNoTracking()
                .Where(t => tagIds.Contains(t.Id))
                .Select(t => new { t.Id, t.TenantId })
                .ToListAsync(ct);

            if (tags.Count != tagIds.Count)
                throw new InvalidOperationException("One or more tags were not found.");

            if (tags.Any(t => t.TenantId != tenantId))
                throw new InvalidOperationException("One or more tags are not in this tenant.");
        }


    }
}

