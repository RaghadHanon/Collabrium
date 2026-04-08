using Application.Common;
using Application.Features.Courses.DTOs;

namespace Application.Interfaces.ReadModels
{
    public interface ICoursesReadModel
    {
        Task<CourseDto?> GetByIdAsync(
           string tenantId,
           string courseId,
           CancellationToken ct = default);

        Task<CourseDetailsDto?> GetDetailsByIdAsync(
            string tenantId,
            string courseId,
            CancellationToken ct = default);

        Task<PagedList<CourseListItemDto, DateTime?>> GetCoursesAsync(
            CoursesSearchAndFilterParametersDto parameters,
            string? currentUserId,
            CancellationToken cancellationToken = default);
        Task<List<CourseTagDto>> GetTagsAsync(
            string tenantId,
            string courseId,
            CancellationToken cancellationToken = default);
    }
}
