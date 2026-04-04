using Domain.Models;

namespace Application.Interfaces.Repositories
{
    public interface IUserCoursesRepository
    {
        Task AddAsync(UserCourse userCourse, CancellationToken ct = default);

        Task UpdateAsync(UserCourse userCourse, CancellationToken ct = default);

        Task<UserCourse?> GetByIdAsync(
            string tenantId,
            string userCourseId,
            CancellationToken ct = default);

        Task<UserCourse?> GetByUserAndCourseAsync(
            string tenantId,
            string userId,
            string courseId,
            CancellationToken ct = default);

        Task<bool> ExistsAsync(
            string tenantId,
            string userId,
            string courseId,
            CancellationToken ct = default);
    }
}
