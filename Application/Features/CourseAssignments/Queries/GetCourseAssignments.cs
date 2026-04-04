using Application.Common;
using Application.Features.CourseAssignments.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using MediatR;
using System.Net;

namespace Application.Features.CourseAssignments.Queries
{
    public sealed class GetCourseAssignments
    {
        public sealed class Query : IRequest<Result<PagedList<CourseAssignmentDto, DateTime?>>>
        {
            public string? CourseId { get; set; }
            public string? AssignedToUserId { get; set; }
            public string? AssignedByUserId { get; set; }

            public DateTime? AssignedAfter { get; set; }
            public DateTime? AssignedBefore { get; set; }

            public int PageSize { get; set; } = 10;
            public DateTime? Cursor { get; set; }
        }

        public sealed class Handler(
            ICourseAssignmentsReadModel courseAssignmentsReadModel,
            ITenantContext tenantContext)
            : IRequestHandler<Query, Result<PagedList<CourseAssignmentDto, DateTime?>>>
        {
            public async Task<Result<PagedList<CourseAssignmentDto, DateTime?>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;

                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<PagedList<CourseAssignmentDto, DateTime?>>.Failure("Tenant not found.", HttpStatusCode.NotFound);

                var parameters = new CourseAssignmentsSearchParametersDto
                {
                    TenantId = tenantId,
                    CourseId = request.CourseId,
                    AssignedToUserId = request.AssignedToUserId,
                    AssignedByUserId = request.AssignedByUserId,
                    AssignedAfter = request.AssignedAfter,
                    AssignedBefore = request.AssignedBefore,
                    PageSize = request.PageSize,
                    Cursor = request.Cursor
                };

                var items = await courseAssignmentsReadModel.GetAssignmentsAsync(parameters, cancellationToken);

                return Result<PagedList<CourseAssignmentDto, DateTime?>>.Success(items, HttpStatusCode.OK);
            }
        }
    }
}
