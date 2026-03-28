using Application.Common;
using Application.Features.CourseAssignments.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using MediatR;
using System.Net;

namespace Application.Features.CourseAssignments.Queries
{
    public sealed class GetMyCourseAssignments
    {
        public sealed class Query : IRequest<Result<List<CourseAssignmentDto>>>
        {
            public required string CurrentUserId { get; set; }
        }

        public sealed class Handler(
            ICourseAssignmentsReadModel courseAssignmentsReadModel,
            ITenantContext tenantContext)
            : IRequestHandler<Query, Result<List<CourseAssignmentDto>>>
        {
            public async Task<Result<List<CourseAssignmentDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;

                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<List<CourseAssignmentDto>>.Failure("Tenant not found.", HttpStatusCode.NotFound);

                var items = await courseAssignmentsReadModel.GetByUserAsync(
                    tenantId,
                    request.CurrentUserId,
                    cancellationToken);

                return Result<List<CourseAssignmentDto>>.Success(items, HttpStatusCode.OK);
            }
        }
    }
}
