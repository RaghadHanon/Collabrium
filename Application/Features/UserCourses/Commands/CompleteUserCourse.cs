using Application.Common;
using Application.Features.UserCourses.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.UserCourses.Commands
{
    public sealed class CompleteUserCourse
    {
        public sealed class Command : IRequest<Result<UserCourseDto>>
        {
            public required string CourseId { get; set; }
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

                var userCourse = await userCoursesRepository.GetByUserAndCourseAsync(
                    tenantId,
                    currentUserId,
                    request.CourseId,
                    cancellationToken);

                await unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    if (userCourse is null)
                    {
                        userCourse = new UserCourse(
                            tenantId: tenantId,
                            userId: currentUserId,
                            courseId: request.CourseId);

                        userCourse.Complete();
                        await userCoursesRepository.AddAsync(userCourse, ct);
                    }
                    else
                    {
                        userCourse.Complete();
                        await userCoursesRepository.UpdateAsync(userCourse, ct);
                    }

                    await unitOfWork.SaveChangesAsync(ct);
                }, cancellationToken);

                var dto = await userCoursesReadModel.GetByUserAndCourseAsync(
                    tenantId,
                    currentUserId,
                    request.CourseId,
                    cancellationToken);

                return dto is null
                    ? Result<UserCourseDto>.Failure("Course completed but could not be loaded.", HttpStatusCode.InternalServerError)
                    : Result<UserCourseDto>.Success(dto, HttpStatusCode.OK);
            }
        }
    }
}
