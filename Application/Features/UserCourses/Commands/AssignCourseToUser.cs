using Application.Common;
using Application.Events;
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
            IUnitOfWork unitOfWork,
            IUsersReadModel usersReadModel,
            IMediator mediator)
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

                if (request.AssignedToUserId == currentUserId)
                    return Result<UserCourseDto>.Failure("You cannot assign a course to yourself.", HttpStatusCode.BadRequest);

            var targetRole = await currentUserAccessor.GetUserRole(request.AssignedToUserId, cancellationToken);

            if (CourseAssignmentRules.IsBlockedTargetRole(targetRole))
                return Result<UserCourseDto>.Failure("You cannot assign a course to a manager.", HttpStatusCode.BadRequest);


                var assignedUser = await usersReadModel.GetUserSnapshotByIdAsync(
                    request.AssignedToUserId,
                    cancellationToken);

                if (assignedUser is null)
                    return Result<UserCourseDto>.Failure("Assigned user not found.", HttpStatusCode.NotFound);

                var assignerUser = await usersReadModel.GetUserSnapshotByIdAsync(
                    currentUserId,
                    cancellationToken);

                if (assignerUser is null)
                    return Result<UserCourseDto>.Failure("Assigner user not found.", HttpStatusCode.NotFound);

                UserCourse? userCourse = null;

                await unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    userCourse = await userCoursesRepository.GetByUserAndCourseAsync(
                        tenantId,
                        request.AssignedToUserId,
                        request.CourseId,
                        ct);

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

                await mediator.Publish(new CourseAssignedEvent(
                    CourseId: course.Id,
                    CourseTitle: course.Title,
                    CourseProvider: course.Provider,
                    AssignerFullName: assignerUser.FullName,
                    AssignedTo: assignedUser
                ), cancellationToken);

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

