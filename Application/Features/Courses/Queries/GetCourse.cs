using Application.Common;
using Application.Features.Courses.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using MediatR;
using System.Net;

namespace Application.Features.Courses.Queries
{
    public sealed class GetCourse
    {
        public sealed class Query : IRequest<Result<CourseDto>>
        {
            public required string CourseId { get; set; }
        }

        public sealed class Handler(
            ICoursesReadModel coursesReadModel,
            ITenantContext tenantContext)
            : IRequestHandler<Query, Result<CourseDto>>
        {
            public async Task<Result<CourseDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;
                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<CourseDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

                var dto = await coursesReadModel.GetByIdAsync(
                    tenantId,
                    request.CourseId,
                    cancellationToken);

                if (dto is null)
                    return Result<CourseDto>.Failure("Course not found.", HttpStatusCode.NotFound);

                return Result<CourseDto>.Success(dto, HttpStatusCode.OK);
            }
        }
    }
}
