using Application.Common;
using Application.Features.UserCourses.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using MediatR;
using System.Net;

namespace Application.Features.UserCourses.Commands
{
    public sealed class UnsaveUserCourse
    {
        public sealed class Command : IRequest<Result<UserCourseDto>>
        {
            public required string CourseId { get; set; }
        }

        public sealed class Handler(
            IUserCoursesRepository userCoursesRepository,
            IUserCoursesReadModel userCoursesReadModel,
            ITenantContext tenantContext,
            ICurrentUserAccessor currentUserAccessor,
            IUnitOfWork unitOfWork)
            : IRequestHandler<Command, Result<UserCourseDto>>
        {
            public async Task<Result<UserCourseDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;
                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<UserCourseDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

                var currentUserId = currentUserAccessor.GetUserId();
                if (string.IsNullOrWhiteSpace(currentUserId))
                    return Result<UserCourseDto>.Failure("Current user not found.", HttpStatusCode.Unauthorized);

                var userCourse = await userCoursesRepository.GetByUserAndCourseAsync(
                    tenantId,
                    currentUserId,
                    request.CourseId,
                    cancellationToken);

                if (userCourse is null)
                    return Result<UserCourseDto>.Failure("User course not found.", HttpStatusCode.NotFound);

                await unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    userCourse.RemoveFromSaved();
                    await userCoursesRepository.UpdateAsync(userCourse, ct);
                    await unitOfWork.SaveChangesAsync(ct);
                }, cancellationToken);

                var dto = await userCoursesReadModel.GetByUserAndCourseAsync(
                    tenantId,
                    currentUserId,
                    request.CourseId,
                    cancellationToken);

                return dto is null
                    ? Result<UserCourseDto>.Failure("Course unsaved but could not be loaded.", HttpStatusCode.InternalServerError)
                    : Result<UserCourseDto>.Success(dto, HttpStatusCode.OK);
            }
        }
    }
}
