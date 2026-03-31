using Application.Common;
using Application.Features.Courses.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using MediatR;
using System.Net;

namespace Application.Features.Courses.Queries
{
    public sealed class GetCourseTags
    {
        public sealed class Query : IRequest<Result<List<CourseTagDto>>>
        {
            public required string CourseId { get; set; }
        }
        public sealed class Handler(
            ICoursesReadModel coursesReadModel,
            ITenantContext tenantContext
        ) : IRequestHandler<GetCourseTags.Query, Result<List<CourseTagDto>>>
        {
            public async Task<Result<List<CourseTagDto>>> Handle(
                GetCourseTags.Query request,
                CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;

                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<List<CourseTagDto>>.Failure(
                        "Tenant not found.",
                        HttpStatusCode.NotFound);

                var tags = await coursesReadModel.GetTagsAsync(
                    tenantId,
                    request.CourseId,
                    cancellationToken);

                return Result<List<CourseTagDto>>.Success(tags, HttpStatusCode.OK);
            }
        }
    }
}
