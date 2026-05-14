using Application.Common;
using Application.Events;
using Application.Features.Users.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Models;
using MediatR;
using System.Net;

namespace Application.Features.UserCourses.Commands;

public sealed class AssignCourseToTeam
{
    public sealed class Command : IRequest<Result<string>>
    {
        public required string CourseId { get; set; }
        public required string TeamId { get; set; }
    }

    public sealed class Handler(
        IUserCoursesRepository userCoursesRepository,
        ITeamsReadModel teamsReadModel,
        ICoursesRepository coursesRepository,
        ITenantContext tenantContext,
        ICurrentUserAccessor currentUserAccessor,
        IUnitOfWork unitOfWork,
        IUsersReadModel usersReadModel,
        IMediator mediator)
        : IRequestHandler<Command, Result<string>>
    {
        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<string>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var currentUserId = currentUserAccessor.GetUserId();
            if (string.IsNullOrWhiteSpace(currentUserId))
                return Result<string>.Failure("Current user not found.", HttpStatusCode.Unauthorized);

            var course = await coursesRepository.GetByIdAsync(
                tenantId,
                request.CourseId,
                cancellationToken);

            if (course is null)
                return Result<string>.Failure("Course not found.", HttpStatusCode.NotFound);

            var team = await teamsReadModel.GetByIdAsync(
                tenantId,
                request.TeamId,
                cancellationToken);

            if (team is null)
                return Result<string>.Failure("Team not found.", HttpStatusCode.NotFound);

            var members = await teamsReadModel.GetTeamMembersSnapshotAsync(
                tenantId,
                request.TeamId,
                cancellationToken);

            if (members is null || members.Count == 0)
                return Result<string>.Failure("Team has no members.", HttpStatusCode.BadRequest);

            var assignerUser = await usersReadModel.GetUserSnapshotByIdAsync(
                currentUserId,
                cancellationToken);

            if (assignerUser is null)
                return Result<string>.Failure("Assigner user not found.", HttpStatusCode.NotFound);

            var assignedMembers = new List<UserSnapshotDto>();

            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                foreach (var member in members)
                {
                    if (string.IsNullOrWhiteSpace(member.Id))
                        continue;

                    var userId = member.Id.Trim();

                    if (string.Equals(userId, currentUserId, StringComparison.Ordinal))
                        continue;

                    var targetRole = await currentUserAccessor.GetUserRole(userId, ct);

                    if (CourseAssignmentRules.IsBlockedTargetRole(targetRole))
                        continue;

                    var userCourse = await userCoursesRepository.GetByUserAndCourseAsync(
                        tenantId,
                        userId,
                        request.CourseId,
                        ct);

                    if (userCourse is null)
                    {
                        userCourse = new UserCourse(
                                tenantId: tenantId,
                                userId: userId,
                                courseId: request.CourseId);

                        userCourse.Assign(currentUserId);
                        await userCoursesRepository.AddAsync(userCourse, ct);
                    }
                    else
                    {
                        userCourse.Assign(currentUserId);
                        await userCoursesRepository.UpdateAsync(userCourse, ct);
                    }

                    assignedMembers.Add(member);
                }

                await unitOfWork.SaveChangesAsync(ct);
            }, cancellationToken);

            if (assignedMembers.Count > 0)
            {
                await mediator.Publish(new CourseAssignedToTeamEvent(
                    CourseId: course.Id,
                    CourseTitle: course.Title,
                    CourseProvider: course.Provider,
                    AssignerFullName: assignerUser.FullName,
                    Members: assignedMembers
                ), cancellationToken);
            }

            return Result<string>.Success(
                "Course assigned to team successfully.",
                HttpStatusCode.OK);
        }
    }
}