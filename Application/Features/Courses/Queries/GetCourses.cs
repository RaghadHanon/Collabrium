using Application.Common;
using Application.Features.Courses.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using AutoMapper;
using MediatR;
using System.Net;

namespace Application.Features.Courses.Queries
{
    public sealed class GetCourses
    {
        public sealed class Query : IRequest<Result<PagedList<CourseDto, DateTime?>>>
        {
            public string? Search { get; set; }
            public string? Provider { get; set; }
            public string? InstructorName { get; set; }
            public string? TenantId { get; set; }

            public Domain.Enums.CourseLevel? Level { get; set; }
            public Domain.Enums.CourseLanguage? Language { get; set; }

            public int? MinEstimatedDurationInHours { get; set; }
            public int? MaxEstimatedDurationInHours { get; set; }

            public bool? HasThumbnail { get; set; }

            public DateTime? CreatedAfter { get; set; }
            public DateTime? CreatedBefore { get; set; }

            public DateTime? UpdatedAfter { get; set; }
            public DateTime? UpdatedBefore { get; set; }

            public DateTime? Cursor { get; set; }
            public int PageSize { get; set; } = 10;
        }

        public sealed class Handler(
            ICoursesReadModel coursesReadModel,
            ITenantContext tenantContext,
            IMapper mapper)
            : IRequestHandler<Query, Result<PagedList<CourseDto, DateTime?>>>
        {
            public async Task<Result<PagedList<CourseDto, DateTime?>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var tenant = tenantContext.CurrentTenant;
                if (tenant is null)
                {
                    return Result<PagedList<CourseDto, DateTime?>>.Failure(
                        "Tenant not found.",
                        HttpStatusCode.NotFound);
                }

                var queryParams = mapper.Map<CoursesSearchAndFilterParametersDto>(request);
                queryParams.TenantId = tenant.Id;

                var pagedCourses = await coursesReadModel.GetCoursesAsync(queryParams, cancellationToken);

                return Result<PagedList<CourseDto, DateTime?>>.Success(
                    pagedCourses,
                    HttpStatusCode.OK);
            }
        }
    }
}
