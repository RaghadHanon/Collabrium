using API.Contracts.Courses;
using FluentValidation;

namespace API.Validators.Courses
{
    public sealed class GetCoursesRequestValidator : AbstractValidator<GetCoursesRequest>
    {
        public GetCoursesRequestValidator()
        {
            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("PageSize must be between 1 and 100.");

            RuleFor(x => x.Search)
                .MaximumLength(200)
                .When(x => !string.IsNullOrWhiteSpace(x.Search));

            RuleFor(x => x.Provider)
                .MaximumLength(200)
                .When(x => !string.IsNullOrWhiteSpace(x.Provider));

            RuleFor(x => x.InstructorName)
                .MaximumLength(200)
                .When(x => !string.IsNullOrWhiteSpace(x.InstructorName));

            RuleFor(x => x.MinEstimatedDurationInHours)
                .GreaterThanOrEqualTo(0)
                .When(x => x.MinEstimatedDurationInHours.HasValue)
                .WithMessage("MinEstimatedDurationInHours must be greater than or equal to 0.");

            RuleFor(x => x.MaxEstimatedDurationInHours)
                .GreaterThanOrEqualTo(0)
                .When(x => x.MaxEstimatedDurationInHours.HasValue)
                .WithMessage("MaxEstimatedDurationInHours must be greater than or equal to 0.");

            RuleFor(x => x.MaxEstimatedDurationInHours)
                .GreaterThanOrEqualTo(x => x.MinEstimatedDurationInHours!.Value)
                .When(x => x.MinEstimatedDurationInHours.HasValue && x.MaxEstimatedDurationInHours.HasValue)
                .WithMessage("MaxEstimatedDurationInHours must be greater than or equal to MinEstimatedDurationInHours.");

            RuleForEach(x => x.TagIds!)
                .NotEmpty()
                .When(x => x.TagIds is not null)
                .WithMessage("TagId must not be empty.");
        }
    }
}
