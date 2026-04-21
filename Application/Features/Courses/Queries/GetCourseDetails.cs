using Application.Common;
using Application.Features.Courses.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using MediatR;
using System.Net;

namespace Application.Features.Courses.Queries
{
    public sealed class GetCourseDetails
    {
        public sealed class Query : IRequest<Result<CourseDetailsDto>>
        {
            public required string CourseId { get; set; }
        }

        public sealed class Handler(
            ICoursesReadModel coursesReadModel,
            ITenantContext tenantContext)
            : IRequestHandler<Query, Result<CourseDetailsDto>>
        {
            public async Task<Result<CourseDetailsDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;
                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<CourseDetailsDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

                var dto = await coursesReadModel.GetDetailsByIdAsync(
                    tenantId,
                    request.CourseId,
                    cancellationToken);

                if (dto is null)
                    return Result<CourseDetailsDto>.Failure("Course not found.", HttpStatusCode.NotFound);

                return Result<CourseDetailsDto>.Success(dto, HttpStatusCode.OK);
            }
        }
    }
}