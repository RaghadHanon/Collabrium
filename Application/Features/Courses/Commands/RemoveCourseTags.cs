using Application.Common;
using Application.Features.Courses.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using MediatR;
using System.Net;

namespace Application.Features.Courses.Commands
{
    public sealed class RemoveCourseTags
    {
        public sealed class Command : IRequest<Result<CourseDto>>
        {
            public required string CourseId { get; set; }
            public List<string> TagIds { get; set; } = new();
        }

        public sealed class Handler(
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
                    return Result<CourseDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

                var course = await coursesRepository.GetByIdAsync(
                    tenantId,
                    request.CourseId,
                    cancellationToken);

                if (course is null)
                    return Result<CourseDto>.Failure("Course not found.", HttpStatusCode.NotFound);

                await unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    foreach (var tagId in request.TagIds)
                    {
                        course.RemoveTag(tagId);
                    }


                    await coursesRepository.RemoveTagsAsync(
                        tenantId,
                        course.Id,
                        request.TagIds,
                        ct);
                }, cancellationToken);

                var dto = await coursesReadModel.GetByIdAsync(
                    tenantId,
                    course.Id,
                    cancellationToken);

                return dto is null
                    ? Result<CourseDto>.Failure("Course tags removed but course could not be loaded.", HttpStatusCode.InternalServerError)
                    : Result<CourseDto>.Success(dto, HttpStatusCode.OK);
            }
        }
    }
}
