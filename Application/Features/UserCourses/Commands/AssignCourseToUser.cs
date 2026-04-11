using Application.Common;
using Application.Features.UserCourses.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Models;
using MediatR;
using System.Net;

namespace Application.Features.UserCourses.Commands
{
    public sealed class AssignCourseToUser
    {

        public sealed class Command : IRequest<Result<UserCourseDto>>
        {
            public required string CourseId { get; set; }
            public required string AssignedToUserId { get; set; }
        }

        public sealed class Handler(
            IUserCoursesRepository userCoursesRepository,
            IUserCoursesReadModel userCoursesReadModel,
            ICoursesRepository coursesRepository,
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

                var course = await coursesRepository.GetByIdAsync(
                    tenantId,
                    request.CourseId,
                    cancellationToken);

                if (course is null)
                    return Result<UserCourseDto>.Failure("Course not found.", HttpStatusCode.NotFound);

                if (string.Equals(request.AssignedToUserId, currentUserId, StringComparison.Ordinal))
                    return Result<UserCourseDto>.Failure("You cannot assign a course to yourself.", HttpStatusCode.BadRequest);

                var targetRole = await currentUserAccessor.GetUserRole(request.AssignedToUserId, cancellationToken);

                if (CourseAssignmentRules.IsBlockedTargetRole(targetRole))
                    return Result<UserCourseDto>.Failure("You cannot assign a course to a manager.", HttpStatusCode.BadRequest);

                var userCourse = await userCoursesRepository.GetByUserAndCourseAsync(
                            tenantId,
                            request.AssignedToUserId,
                            request.CourseId,
                            cancellationToken);

                await unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    if (userCourse is null)
                    {
                        userCourse = new UserCourse(
                            tenantId: tenantId,
                            userId: request.AssignedToUserId,
                            courseId: request.CourseId);

                        userCourse.Assign(currentUserId);
                        await userCoursesRepository.AddAsync(userCourse, ct);
                    }
                    else
                    {
                        userCourse.Assign(currentUserId);
                        await userCoursesRepository.UpdateAsync(userCourse, ct);
                    }

                    await unitOfWork.SaveChangesAsync(ct);
                }, cancellationToken);

                var dto = await userCoursesReadModel.GetByUserAndCourseAsync(
                    tenantId,
                    request.AssignedToUserId,
                    request.CourseId,
                    cancellationToken);

                return dto is null
                    ? Result<UserCourseDto>.Failure("Course assigned but could not be loaded.", HttpStatusCode.InternalServerError)
                    : Result<UserCourseDto>.Success(dto, HttpStatusCode.OK);
            }
        }
    }
}

