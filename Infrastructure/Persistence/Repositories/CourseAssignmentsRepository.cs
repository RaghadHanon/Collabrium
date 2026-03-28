using Application.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public sealed class CourseAssignmentsRepository : ICourseAssignmentsRepository
    {
        private readonly AppDbContext _dbContext;
        public CourseAssignmentsRepository(AppDbContext dbContext)
            => _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        public async Task AddAsync(CourseAssignment assignment, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(assignment);
            var entity = CourseAssignmentMapper.ToEntity(assignment);
            await _dbContext.CourseAssignments.AddAsync(entity, ct);
        }

        public Task<bool> ExistsAsync(
            string tenantId,
            string courseId,
            string assignedToUserId,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(tenantId)
                || string.IsNullOrWhiteSpace(courseId)
                || string.IsNullOrWhiteSpace(assignedToUserId))
                return Task.FromResult(false);

            var tId = tenantId.Trim();
            var cId = courseId.Trim();
            var uId = assignedToUserId.Trim();
            return _dbContext.CourseAssignments
                .AsNoTracking()
                .AnyAsync(a =>
                           a.TenantId == tId
                           && a.CourseId == cId
                           && a.AssignedToUserId == uId, ct);
        }

        public async Task<CourseAssignment?> GetByCourseAndUserAsync(
          string tenantId,
          string courseId,
          string assignedToUserId,
          CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(tenantId) ||
                string.IsNullOrWhiteSpace(courseId) ||
                string.IsNullOrWhiteSpace(assignedToUserId))
                return null;

            var tId = tenantId.Trim();
            var cId = courseId.Trim();
            var uId = assignedToUserId.Trim();

            var entity = await _dbContext.CourseAssignments
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.TenantId == tId &&
                    x.CourseId == cId &&
                    x.AssignedToUserId == uId, ct);

            return entity is null
                ? null
                : CourseAssignmentMapper.ToDomain(entity);
        }


        public async Task<CourseAssignment?> GetByIdAsync(
            string tenantId,
            string assignmentId,
            CancellationToken ct = default)
        {
            if (!TryNormalize(tenantId, assignmentId, out var tId, out var aId))
                return null;

            var entity = await _dbContext.CourseAssignments
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenantId == tId && x.Id == aId, ct);

            return entity is null
                ? null
                : CourseAssignmentMapper.ToDomain(entity);
        }


        public async Task<bool> RemoveAsync(
           string tenantId,
           string assignmentId,
           CancellationToken ct = default)
        {
            if (!TryNormalize(tenantId, assignmentId, out var tId, out var aId))
                return false;

            var entity = await _dbContext.CourseAssignments
                .FirstOrDefaultAsync(x => x.TenantId == tId && x.Id == aId, ct);

            if (entity is null)
                return false;

            _dbContext.CourseAssignments.Remove(entity);
            return true;
        }


        public async Task UpdateAsync(CourseAssignment assignment, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(assignment);

            var entity = await _dbContext.CourseAssignments
                .FirstOrDefaultAsync(x =>
                    x.Id == assignment.Id &&
                    x.TenantId == assignment.TenantId, ct);

            if (entity is null)
                throw new InvalidOperationException("Course assignment not found.");

            CourseAssignmentMapper.UpdateEntity(entity, assignment);
        }

        private static bool TryNormalize(string? tenantId, string? assignmentId, out string tId, out string aId)
        {
            tId = tenantId?.Trim() ?? "";
            aId = assignmentId?.Trim() ?? "";

            return !string.IsNullOrWhiteSpace(tId) &&
                   !string.IsNullOrWhiteSpace(aId);
        }
    }
}
