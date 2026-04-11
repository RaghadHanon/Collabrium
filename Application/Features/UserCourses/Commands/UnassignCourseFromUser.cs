using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.UserCourses.Commands
{
    public sealed class UnassignCourseFromUser
    {
        public sealed class Command : IRequest<Result<string>>
        {
            public required string CourseId { get; set; }
            public required string AssignedToUserId { get; set; }
        }

        public sealed class Handler(
            IUserCoursesRepository userCoursesRepository,
            ICoursesRepository coursesRepository,
            ITenantContext tenantContext,
            IUnitOfWork unitOfWork)
            : IRequestHandler<Command, Result<string>>
        {
            public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;
                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<string>.Failure("Tenant not found.", HttpStatusCode.NotFound);

                var course = await coursesRepository.GetByIdAsync(
                    tenantId,
                    request.CourseId,
                    cancellationToken);

                if (course is null)
                    return Result<string>.Failure("Course not found.", HttpStatusCode.NotFound);

                var userCourse = await userCoursesRepository.GetByUserAndCourseAsync(
                    tenantId,
                    request.AssignedToUserId,
                    request.CourseId,
                    cancellationToken);

                if (userCourse is null)
                    return Result<string>.Failure("User course not found.", HttpStatusCode.NotFound);

                await unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    if (userCourse.CanBeDeletedWhenUnassigned())
                    {
                        await userCoursesRepository.DeleteByUserAndCourseAsync(
                            tenantId,
                            request.AssignedToUserId,
                            request.CourseId,
                            ct);
                    }
                    else
                    {
                        userCourse.Unassign();
                        await userCoursesRepository.UpdateAsync(userCourse, ct);
                    }

                    await unitOfWork.SaveChangesAsync(ct);
                }, cancellationToken);

                return Result<string>.Success("Course unassigned successfully.", HttpStatusCode.OK);
            }
        }
    }
}
