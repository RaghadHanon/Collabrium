using Application.Common;
using Application.Features.Courses.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Domain.Enums;
using MediatR;
using System.Net;

namespace Application.Features.Courses.Commands
{
    public sealed class UpdateCourse
    {
        public sealed class Command : IRequest<Result<CourseDto>>
        {
            public required string CourseId { get; set; }
            public string? Title { get; set; }
            public string? Description { get; set; }
            public string? Url { get; set; }
            public string? Provider { get; set; }
            public string? InstructorName { get; set; }

            public CourseLevel? Level { get; set; }
            public CourseLanguage? Language { get; set; }

            public int? EstimatedDurationInHours { get; set; }

        }
        public class Handler(
            ICoursesRepository coursesRepository,
            ICoursesReadModel coursesReadModel,
            ITenantContext tenantContext,
            IUnitOfWork unitOfWork)
            : IRequestHandler<Command, Result<CourseDto>>
        {
            public async Task<Result<CourseDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;
                if (string.IsNullOrWhiteSpace(tenantId))
                {
                    return Result<CourseDto>.Failure("Tenant not Found.", HttpStatusCode.NotFound);
                }
                var course = await coursesRepository.GetByIdAsync(
                   tenantId,
                   request.CourseId,
                   cancellationToken);
                if (course == null)
                {
                    return Result<CourseDto>.Failure("Course not found.", HttpStatusCode.NotFound);
                }
                var titleDesison = PatchHelpers.DecideStringPatch(request.Title);
                if (titleDesison.Action == PatchHelpers.PatchStringAction.Set)
                {
                    var newTitle = titleDesison.Value!;
                }
                var finalDuration = request.EstimatedDurationInHours ?? course.EstimatedDurationInHours;
                if (finalDuration.HasValue && finalDuration.Value <= 0)
                    return Result<CourseDto>.Failure("EstimatedDurationInHours must be greater than 0.", HttpStatusCode.BadRequest);

                await unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    course.UpdateDetails(
                        title: PatchHelpers.ApplyStringPatch(course.Title, request.Title) ?? course.Title,
                        description: PatchHelpers.ApplyStringPatch(course.Description, request.Description) ?? course.Description,
                        url: PatchHelpers.ApplyStringPatch(course.Url, request.Url) ?? course.Url,
                        provider: PatchHelpers.ApplyStringPatch(course.Provider, request.Provider) ?? course.Provider,
                        level: request.Level ?? course.Level,
                        language: request.Language ?? course.Language,
                        instructorName: PatchHelpers.ApplyStringPatch(course.InstructorName, request.InstructorName),
                        estimatedDurationInHours: finalDuration
                    );

                    await coursesRepository.UpdateAsync(course, ct);
                }, cancellationToken);

                var dto = await coursesReadModel.GetByIdAsync(
                    tenantId,
                    course.Id,
                    cancellationToken);

                return dto is null
                    ? Result<CourseDto>.Failure("Course updated but could not be loaded.", HttpStatusCode.InternalServerError)
                    : Result<CourseDto>.Success(dto, HttpStatusCode.OK);
            }

        }
    }
}

