using Application.Common;
using Application.Features.UserCourses.DTOs;
using Domain.Models;

namespace Application.Interfaces.Repositories;

public interface IUserCoursesRepository
{
    Task AddAsync(UserCourse userCourse, CancellationToken cancellationToken = default);

    Task UpdateAsync(UserCourse userCourse, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        string tenantId,
        string userCourseId,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteByUserAndCourseAsync(
        string tenantId,
        string userId,
        string courseId,
        CancellationToken cancellationToken = default);

    Task<UserCourse?> GetByIdAsync(
        string tenantId,
        string userCourseId,
        CancellationToken cancellationToken = default);

    Task<UserCourse?> GetByUserAndCourseAsync(
        string tenantId,
        string userId,
        string courseId,
        CancellationToken cancellationToken = default);

    Task<PagedList<UserCourse, DateTime?>> GetPagedAsync(
        UserCoursesSearchAndFilterParametersDto parameters,
        CancellationToken cancellationToken = default);


    Task<bool> ExistsAsync(
        string tenantId,
        string userId,
        string courseId,
        CancellationToken ct = default);
}
