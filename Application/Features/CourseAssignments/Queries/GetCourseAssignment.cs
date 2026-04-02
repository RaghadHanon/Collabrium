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
        public sealed class Query : IRequest<Result<CourseAssignmentDetailsDto>>
        {
            public required string AssignmentId { get; set; }
        }

        public sealed class Handler(
            ICourseAssignmentsReadModel courseAssignmentsReadModel,
            ITenantContext tenantContext)
            : IRequestHandler<Query, Result<CourseAssignmentDetailsDto>>
        {
            public async Task<Result<CourseAssignmentDetailsDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;

                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<CourseAssignmentDetailsDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

                var dto = await courseAssignmentsReadModel.GetDetailsByIdAsync(
                    tenantId,
                    request.AssignmentId,
                    cancellationToken);

                return dto is null
                    ? Result<CourseAssignmentDetailsDto>.Failure("Course assignment not found.", HttpStatusCode.NotFound)
                    : Result<CourseAssignmentDetailsDto>.Success(dto, HttpStatusCode.OK);
            }
        }
    }
}

