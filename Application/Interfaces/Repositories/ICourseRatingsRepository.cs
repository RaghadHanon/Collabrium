using Application.Common;
using Application.Features.CourseRatings.DTOs;
using Domain.Models;

namespace Application.Interfaces.Repositories
{
    public interface ICourseRatingsRepository
    {
        Task AddAsync(CourseRating courseRating, CancellationToken cancellationToken = default);

        Task UpdateAsync(CourseRating courseRating, CancellationToken cancellationToken = default);

        Task<bool> RemoveAsync(
            string tenantId,
            string ratingId,
            CancellationToken cancellationToken = default);

        Task<CourseRating?> GetByIdAsync(
            string tenantId,
            string ratingId,
            CancellationToken cancellationToken = default);

        Task<CourseRating?> GetByUserAndCourseAsync(
            string tenantId,
            string userId,
            string courseId,
            CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(
            string tenantId,
            string ratingId,
            CancellationToken cancellationToken = default);

        Task<PagedList<CourseRating, DateTime?>> GetPagedAsync(
            CourseRatingsSearchAndFilterParametersDto parameters,
            CancellationToken cancellationToken = default);
    }
}
