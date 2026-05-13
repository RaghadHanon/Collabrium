using Application.Common;
using Application.Features.UserCourses.DTOs;

namespace Application.Interfaces.ReadModels;

public interface IUserCoursesReadModel
{
    Task<AssignedByMeStatsDto> GetAssignedByMeStatsAsync(
        string tenantId,
        string currentUserId,
        CancellationToken cancellationToken = default);
    Task<MyLearningSummaryDto> GetMyLearningSummaryAsync(
        string tenantId,
        string userId,
        CancellationToken ct = default);
    Task<UserCourseDto?> GetByIdAsync(
        string tenantId,
        string userCourseId,
        CancellationToken ct = default);

    Task<UserCourseDto?> GetByUserAndCourseAsync(
        string tenantId,
        string userId,
        string courseId,
        CancellationToken ct = default);

    Task<UserCourseDetailsDto?> GetDetailsByIdAsync(
        string tenantId,
        string userCourseId,
        CancellationToken ct = default);

    Task<List<UserCourseDto>> GetByUserAsync(
        string tenantId,
        string userId,
        CancellationToken ct = default);

    Task<PagedList<UserCourseDto, DateTime?>> GetUserCoursesAsync(
        UserCoursesSearchAndFilterParametersDto parameters,
        CancellationToken ct = default);

    Task<PagedList<AssignedUserCourseDto, DateTime?>> GetAssignedByUserAsync(
        AssignedUserCoursesSearchAndFilterParametersDto parameters,
        CancellationToken cancellationToken = default);
}
