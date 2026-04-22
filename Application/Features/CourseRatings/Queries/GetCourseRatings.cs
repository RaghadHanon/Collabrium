using Application.Common;
using Application.Features.CourseRatings.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using AutoMapper;
using MediatR;
using System.Net;

namespace Application.Features.CourseRatings.Queries
{
    public sealed class GetCourseRatings
    {
        public sealed class Query : IRequest<Result<PagedList<CourseRatingDto, DateTime?>>>
        {
            public required string CourseId { get; set; }

            public int? MinRating { get; set; }
            public int? MaxRating { get; set; }

            public DateTime? CreatedAfter { get; set; }
            public DateTime? CreatedBefore { get; set; }

            public DateTime? UpdatedAfter { get; set; }
            public DateTime? UpdatedBefore { get; set; }

            public int PageSize { get; set; } = 10;
            public DateTime? Cursor { get; set; }
        }

        public sealed class Handler(
            ICourseRatingsReadModel courseRatingsReadModel,
            ICoursesRepository coursesRepository,
            ITenantContext tenantContext,
            IMapper mapper)
            : IRequestHandler<Query, Result<PagedList<CourseRatingDto, DateTime?>>>
        {
            public async Task<Result<PagedList<CourseRatingDto, DateTime?>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.CurrentTenant?.Id;
                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<PagedList<CourseRatingDto, DateTime?>>.Failure("Tenant not found.", HttpStatusCode.NotFound);

                var course = await coursesRepository.GetByIdAsync(
                    tenantId,
                    request.CourseId,
                    cancellationToken);

                if (course is null)
                    return Result<PagedList<CourseRatingDto, DateTime?>>.Failure("Course not found.", HttpStatusCode.NotFound);

                var parameters = mapper.Map<CourseRatingsSearchAndFilterParametersDto>(request);
                parameters.TenantId = tenantId;
                parameters.CourseId = request.CourseId;

                var result = await courseRatingsReadModel.GetCourseRatingsAsync(parameters, cancellationToken);

                return Result<PagedList<CourseRatingDto, DateTime?>>.Success(result, HttpStatusCode.OK);
            }
        }
    }
}
