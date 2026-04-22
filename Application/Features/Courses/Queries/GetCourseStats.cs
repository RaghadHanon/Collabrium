using Application.Common;
using Application.Features.Courses.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using MediatR;
using System.Net;

namespace Application.Features.Courses.Queries
{
    public sealed class GetCourseStats
    {
        public sealed class Query : IRequest<Result<CourseStatsDto>>
        {
            public string CourseId { get; set; } = null!;
        }
        public sealed class Handler(
            ICoursesReadModel coursesReadModel,
            ITenantContext tenantContext)
            : IRequestHandler<Query, Result<CourseStatsDto>>
        {
            public async Task<Result<CourseStatsDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;
                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<CourseStatsDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);
                var courseStats = await coursesReadModel.GetStatsByIdAsync(
                    tenantId,
                    request.CourseId,
                    cancellationToken);
                if (courseStats is null)
                    return Result<CourseStatsDto>.Failure("Course not found.", HttpStatusCode.NotFound);
                return Result<CourseStatsDto>.Success(courseStats, HttpStatusCode.OK);
            }
        }
    }
}
