using API.Contracts.CourseRatings;
using FluentValidation;

namespace API.Validators.CourseRatings;

public sealed class GetCourseRatingsRequestValidator : AbstractValidator<GetCourseRatingsRequest>
{
    public GetCourseRatingsRequestValidator()
    {
        // Rating range
        RuleFor(x => x.MinRating)
            .InclusiveBetween(1, 5)
            .When(x => x.MinRating.HasValue)
            .WithMessage("MinRating must be between 1 and 5.");

        RuleFor(x => x.MaxRating)
            .InclusiveBetween(1, 5)
            .When(x => x.MaxRating.HasValue)
            .WithMessage("MaxRating must be between 1 and 5.");

        RuleFor(x => x)
            .Must(x => !x.MinRating.HasValue || !x.MaxRating.HasValue || x.MinRating <= x.MaxRating)
            .WithMessage("MinRating must be less than or equal to MaxRating.");

        // Created date range
        RuleFor(x => x)
            .Must(x => !x.CreatedAfter.HasValue || !x.CreatedBefore.HasValue || x.CreatedAfter <= x.CreatedBefore)
            .WithMessage("CreatedAfter must be less than or equal to CreatedBefore.");

        // Updated date range
        RuleFor(x => x)
            .Must(x => !x.UpdatedAfter.HasValue || !x.UpdatedBefore.HasValue || x.UpdatedAfter <= x.UpdatedBefore)
            .WithMessage("UpdatedAfter must be less than or equal to UpdatedBefore.");

        // Pagination
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100.");
    }
}
