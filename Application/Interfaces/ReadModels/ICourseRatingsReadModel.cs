using Application.Common;
using Application.Features.CourseRatings.DTOs;

namespace Application.Interfaces.ReadModels;

public interface ICourseRatingsReadModel
{
    Task<CourseRatingDto?> GetByIdAsync(
        string tenantId,
        string ratingId,
        CancellationToken ct = default);

    Task<CourseRatingDto?> GetByUserAndCourseAsync(
        string tenantId,
        string userId,
        string courseId,
        CancellationToken ct = default);

    Task<CourseRatingDetailsDto?> GetDetailsByIdAsync(
        string tenantId,
        string ratingId,
        CancellationToken ct = default);

    Task<PagedList<CourseRatingDto, DateTime?>> GetCourseRatingsAsync(
        CourseRatingsSearchAndFilterParametersDto parameters,
        CancellationToken ct = default);
}
