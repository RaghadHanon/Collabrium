using Application.Common;
using Application.Features.UserCourses.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Services;
using MediatR;
using System.Net;

namespace Application.Features.UserCourses.Queries
{
    public sealed class GetMyUserCoursesSummary
    {
        public sealed class Query : IRequest<Result<MyLearningSummaryDto>>
        {

        }
        public sealed class Handler(
           IUserCoursesReadModel userCoursesReadModel,
           ITenantContext tenantContext,
           ICurrentUserAccessor currentUserAccessor)
           : IRequestHandler<Query, Result<MyLearningSummaryDto>>
        {
            public async Task<Result<MyLearningSummaryDto>> Handle(
                Query request,
                CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;
                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<MyLearningSummaryDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

                var currentUserId = currentUserAccessor.GetUserId();
                if (string.IsNullOrWhiteSpace(currentUserId))
                    return Result<MyLearningSummaryDto>.Failure("Current user not found.", HttpStatusCode.Unauthorized);

                var dto = await userCoursesReadModel.GetMyLearningSummaryAsync(
                    tenantId,
                    currentUserId,
                    cancellationToken);

                return Result<MyLearningSummaryDto>.Success(dto, HttpStatusCode.OK);
            }
        }
    }
}

