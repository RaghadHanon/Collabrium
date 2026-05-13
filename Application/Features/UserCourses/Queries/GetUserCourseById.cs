using Application.Common;
using Application.Features.UserCourses.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Services;
using MediatR;
using System.Net;

namespace Application.Features.UserCourses.Queries;

public sealed class GetUserCourseById
{
    public sealed class Query : IRequest<Result<UserCourseDetailsDto>>
    {
        public required string UserCourseId { get; set; }
    }

    public sealed class Handler(
        IUserCoursesReadModel userCoursesReadModel,
        ITenantContext tenantContext,
        ICurrentUserAccessor currentUserAccessor)
        : IRequestHandler<Query, Result<UserCourseDetailsDto>>
    {
        public async Task<Result<UserCourseDetailsDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<UserCourseDetailsDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var currentUserId = currentUserAccessor.GetUserId();
            if (string.IsNullOrWhiteSpace(currentUserId))
                return Result<UserCourseDetailsDto>.Failure("Current user not found.", HttpStatusCode.Unauthorized);

            var dto = await userCoursesReadModel.GetDetailsByIdAsync(
                tenantId,
                request.UserCourseId,
                cancellationToken);

            return dto is null
                ? Result<UserCourseDetailsDto>.Failure("User course not found.", HttpStatusCode.NotFound)
                : !string.Equals(dto.UserId, currentUserId, StringComparison.Ordinal)
                ? Result<UserCourseDetailsDto>.Failure("User course not found.", HttpStatusCode.NotFound)
                : Result<UserCourseDetailsDto>.Success(dto, HttpStatusCode.OK);
        }
    }
}

