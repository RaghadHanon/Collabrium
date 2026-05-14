using Application.Common;
using Application.Features.UserCourses.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Services;
using MediatR;
using System.Net;

namespace Application.Features.UserCourses.Queries;

public sealed class GetAssignedByMeStats
{
    public sealed class Query : IRequest<Result<AssignedByMeStatsDto>>
    {
    }

    public sealed class Handler(
        IUserCoursesReadModel userCoursesReadModel,
        ITenantContext tenantContext,
        ICurrentUserAccessor currentUserAccessor)
        : IRequestHandler<Query, Result<AssignedByMeStatsDto>>
    {
        public async Task<Result<AssignedByMeStatsDto>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<AssignedByMeStatsDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var currentUserId = currentUserAccessor.GetUserId();
            if (string.IsNullOrWhiteSpace(currentUserId))
                return Result<AssignedByMeStatsDto>.Failure("User not found.", HttpStatusCode.Unauthorized);

            var dto = await userCoursesReadModel.GetAssignedByMeStatsAsync(
                tenantId,
                currentUserId,
                cancellationToken);

            return Result<AssignedByMeStatsDto>.Success(dto, HttpStatusCode.OK);
        }
    }
}
