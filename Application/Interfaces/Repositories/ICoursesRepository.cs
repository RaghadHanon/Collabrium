using Application.Common;
using Application.Features.Courses.DTOs;
using Domain.Models;

namespace Application.Interfaces.Repositories;

public interface ICoursesRepository
{
    Task AddAsync(Course course, CancellationToken cancellationToken = default);

    Task<Course?> GetByIdAsync(
        string tenantId,
        string courseId,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        string tenantId,
        string courseId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        string tenantId,
        string courseId,
        CancellationToken cancellationToken = default);


    Task UpdateAsync(
        Course course,
        CancellationToken cancellationToken = default);


    Task AddTagsAsync(
        string tenantId,
        string courseId,
        IReadOnlyCollection<string> tagIds,
        CancellationToken cancellationToken = default);

    Task RemoveTagsAsync(
        string tenantId,
        string courseId,
        IReadOnlyCollection<string> tagIds,
        CancellationToken cancellationToken = default);

    Task SetTagsAsync(
        string tenantId,
        string courseId,
        IReadOnlyCollection<string> tagIds,
        CancellationToken cancellationToken = default);
    Task<PagedList<Course, DateTime?>> GetPagedAsync(
         CoursesSearchAndFilterParametersDto parameters,
               CancellationToken cancellationToken = default);
}
