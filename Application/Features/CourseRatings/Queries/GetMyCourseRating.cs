using Application.Common;
using Application.Features.CourseRatings.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using MediatR;
using System.Net;

namespace Application.Features.CourseRatings.Queries;

public sealed class GetMyCourseRating
{
    public sealed class Query : IRequest<Result<CourseRatingDto>>
    {
        public required string CourseId { get; set; }
    }

    public sealed class Handler(
        ICourseRatingsReadModel courseRatingsReadModel,
        ICoursesRepository coursesRepository,
        ITenantContext tenantContext,
        ICurrentUserAccessor currentUserAccessor)
        : IRequestHandler<Query, Result<CourseRatingDto>>
    {
        public async Task<Result<CourseRatingDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.CurrentTenant?.Id;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<CourseRatingDto>.Failure("Tenant not found.", HttpStatusCode.NotFound);

            var currentUserId = currentUserAccessor.GetUserId();
            if (string.IsNullOrWhiteSpace(currentUserId))
                return Result<CourseRatingDto>.Failure("Current user not found.", HttpStatusCode.Unauthorized);

            var course = await coursesRepository.GetByIdAsync(
                tenantId,
                request.CourseId,
                cancellationToken);

            if (course is null)
                return Result<CourseRatingDto>.Failure("Course not found.", HttpStatusCode.NotFound);

            var dto = await courseRatingsReadModel.GetByUserAndCourseAsync(
                tenantId,
                currentUserId,
                request.CourseId,
                cancellationToken);

            return dto is null
                ? Result<CourseRatingDto>.Failure("Course rating not found.", HttpStatusCode.NotFound)
                : Result<CourseRatingDto>.Success(dto, HttpStatusCode.OK);
        }
    }
}
