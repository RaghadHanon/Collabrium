using Application.Common;
using Application.Features.CourseAssignments.DTOs;

namespace Application.Interfaces.ReadModels
{
    public interface ICourseAssignmentsReadModel
    {
        Task<CourseAssignmentDto?> GetByIdAsync(
                    string tenantId,
                    string assignmentId,
                    CancellationToken ct = default);

        Task<PagedList<CourseAssignmentDto, DateTime?>> GetAssignmentsAsync(
            CourseAssignmentsSearchParametersDto parameters,
            CancellationToken ct = default);

        Task<List<CourseAssignmentDto>> GetByUserAsync(
            string tenantId,
            string userId,
            CancellationToken ct = default);

        Task<List<CourseAssignmentDto>> GetByCourseAsync(
            string tenantId,
            string courseId,
            CancellationToken ct = default);
    }
}
