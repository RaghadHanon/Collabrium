using Application.Common;
using Application.Features.CourseAssignments.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using MediatR;
using System.Net;

namespace Application.Features.CourseAssignments.Queries
{
    public sealed class GetCourseAssignment
    {
        public sealed class Query : IRequest<Result<CourseAssignmentDto>>
        {
            public required string AssignmentId { get; set; }
        }

        public sealed class Handler(
            ICourseAssignmentsReadModel courseAssignmentsReadModel,
            ITenantContext tenantContext)
            : IRequestHandler<Query, Result<CourseAssignmentDto>>
        {
            public async Task<Result<CourseAssignmentDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;

                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<CourseAssignmentDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

                var dto = await courseAssignmentsReadModel.GetByIdAsync(
                    tenantId,
                    request.AssignmentId,
                    cancellationToken);

                return dto is null
                    ? Result<CourseAssignmentDto>.Failure("Course assignment not found.", HttpStatusCode.NotFound)
                    : Result<CourseAssignmentDto>.Success(dto, HttpStatusCode.OK);
            }
        }
    }
}
