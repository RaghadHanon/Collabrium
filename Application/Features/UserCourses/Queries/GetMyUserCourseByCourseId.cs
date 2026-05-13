using Application.Common;
using Application.Features.UserCourses.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Services;
using MediatR;
using System.Net;

namespace Application.Features.UserCourses.Queries;

public sealed class GetMyUserCourseByCourseId
{
    public sealed class Query : IRequest<Result<UserCourseDto>>
    {
        public required string CourseId { get; set; }
    }

    public sealed class Handler(
        IUserCoursesReadModel userCoursesReadModel,
        ITenantContext tenantContext,
        ICurrentUserAccessor currentUserAccessor)
        : IRequestHandler<Query, Result<UserCourseDto>>
    {
        public async Task<Result<UserCourseDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<UserCourseDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var currentUserId = currentUserAccessor.GetUserId();
            if (string.IsNullOrWhiteSpace(currentUserId))
                return Result<UserCourseDto>.Failure("Current user not found.", HttpStatusCode.Unauthorized);

            var dto = await userCoursesReadModel.GetByUserAndCourseAsync(
                tenantId,
                currentUserId,
                request.CourseId,
                cancellationToken);

            return dto is null
                ? Result<UserCourseDto>.Failure("User course not found.", HttpStatusCode.NotFound)
                : Result<UserCourseDto>.Success(dto, HttpStatusCode.OK);
        }
    }
}
