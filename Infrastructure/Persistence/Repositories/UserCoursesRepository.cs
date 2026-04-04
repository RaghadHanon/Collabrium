using Application.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
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
    }
}
