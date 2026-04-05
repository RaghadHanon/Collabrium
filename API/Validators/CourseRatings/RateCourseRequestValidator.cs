using API.Contracts.CourseRatings;
using FluentValidation;

namespace API.Validators.CourseRatings
{
    public sealed class RateCourseRequestValidator : AbstractValidator<RateCourseRequest>
    {
        public RateCourseRequestValidator()
        {
            RuleFor(x => x.Rating)
                .InclusiveBetween(1, 5)
                .WithMessage("Rating must be between 1 and 5.");
        }
    }
}
