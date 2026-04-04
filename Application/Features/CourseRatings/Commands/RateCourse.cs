using Application.Common;
using Application.Features.CourseRatings.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Models;
using MediatR;
using System.Net;

namespace Application.Features.CourseRatings.Commands
{
    public sealed class RateCourse
    {
        public sealed class Command : IRequest<Result<CourseRatingDto>>
        {
            public required string CourseId { get; set; }
            public required int Rating { get; set; }
        }

        public sealed class Handler(
            ICourseRatingsRepository courseRatingsRepository,
            ICourseRatingsReadModel courseRatingsReadModel,
            ICoursesRepository coursesRepository,
            ITenantContext tenantContext,
            ICurrentUserAccessor currentUserAccessor,
            IUnitOfWork unitOfWork)
            : IRequestHandler<Command, Result<CourseRatingDto>>
        {
            public async Task<Result<CourseRatingDto>> Handle(Command request, CancellationToken cancellationToken)
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

                var existing = await courseRatingsRepository.GetByUserAndCourseAsync(
                    tenantId,
                    currentUserId,
                    request.CourseId,
                    cancellationToken);

                await unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    if (existing is null)
                    {
                        var rating = new CourseRating(
                            tenantId: tenantId,
                            userId: currentUserId,
                            courseId: request.CourseId,
                            rating: request.Rating);

                        await courseRatingsRepository.AddAsync(rating, ct);
                    }
                    else
                    {
                        existing.Update(request.Rating);
                        await courseRatingsRepository.UpdateAsync(existing, ct);
                    }

                    await unitOfWork.SaveChangesAsync(ct);
                }, cancellationToken);

                var dto = await courseRatingsReadModel.GetByUserAndCourseAsync(
                    tenantId,
                    currentUserId,
                    request.CourseId,
                    cancellationToken);

                return dto is null
                    ? Result<CourseRatingDto>.Failure("Course rating saved but could not be loaded.", HttpStatusCode.InternalServerError)
                    : Result<CourseRatingDto>.Success(dto, HttpStatusCode.OK);
            }
        }
    }
}
