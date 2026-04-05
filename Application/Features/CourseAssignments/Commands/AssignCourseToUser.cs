using Application.Common;
using Application.Features.CourseAssignments.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Models;
using MediatR;
using System.Net;

namespace Application.Features.CourseAssignments.Commands
{
    public sealed class AssignCourseToUser
    {
        public sealed class Command : IRequest<Result<CourseAssignmentDto>>
        {
            public required string CourseId { get; set; }
            public required string AssignedToUserId { get; set; }
        }

        public sealed class Handler(
            ICourseAssignmentsRepository courseAssignmentsRepository,
            ICourseAssignmentsReadModel courseAssignmentsReadModel,
            ICoursesRepository coursesRepository,
            IUserCoursesRepository userCoursesRepository,
            ITenantContext tenantContext,
            ICurrentUserAccessor currentUserAccessor,
            IUnitOfWork unitOfWork)
            : IRequestHandler<Command, Result<CourseAssignmentDto>>
        {
            public async Task<Result<CourseAssignmentDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;
                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<CourseAssignmentDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

                var currentUserId = currentUserAccessor.GetUserId();
                if (string.IsNullOrWhiteSpace(currentUserId))
                    return Result<CourseAssignmentDto>.Failure("Current user not found.", HttpStatusCode.Unauthorized);

                var course = await coursesRepository.GetByIdAsync(
                    tenantId,
                    request.CourseId,
                    cancellationToken);

                if (course is null)
                    return Result<CourseAssignmentDto>.Failure("Course not found.", HttpStatusCode.NotFound);

                var alreadyExists = await courseAssignmentsRepository.ExistsAsync(
                    tenantId,
                    request.CourseId,
                    request.AssignedToUserId,
                    cancellationToken);

                if (alreadyExists)
                {
                    return Result<CourseAssignmentDto>.Failure(
                        "This course is already assigned to the user.",
                        HttpStatusCode.BadRequest);
                }

                CourseAssignment assignment = null!;

                await unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    assignment = new CourseAssignment(
                        tenantId: tenantId,
                        courseId: request.CourseId,
                        assignedToUserId: request.AssignedToUserId,
                        assignedByUserId: currentUserId);

                    await courseAssignmentsRepository.AddAsync(assignment, ct);

                    var existingUserCourse = await userCoursesRepository.GetByUserAndCourseAsync(
                        tenantId,
                        request.AssignedToUserId,
                        request.CourseId,
                        ct);

                    if (existingUserCourse is null)
                    {
                        var userCourse = new UserCourse(
                            tenantId: tenantId,
                            userId: request.AssignedToUserId,
                            courseId: request.CourseId);

                        await userCoursesRepository.AddAsync(userCourse, ct);
                    }

                    await unitOfWork.SaveChangesAsync(ct);
                }, cancellationToken);

                var dto = await courseAssignmentsReadModel.GetByIdAsync(
                    tenantId,
                    assignment.Id,
                    cancellationToken);

                return dto is null
                    ? Result<CourseAssignmentDto>.Failure(
                        "Course assignment created but could not be loaded.",
                        HttpStatusCode.InternalServerError)
                    : Result<CourseAssignmentDto>.Success(dto, HttpStatusCode.Created);
            }
        }
    }
}
