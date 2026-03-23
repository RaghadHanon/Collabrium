using API.Contracts.Courses;
using FluentValidation;

namespace API.Validators.Courses
{
    public sealed class UpdateCourseRequestValidator : AbstractValidator<UpdateCourseRequest>
    {
        public UpdateCourseRequestValidator()
        {
            RuleFor(x => x.Title)
                .MaximumLength(200)
                .When(x => !string.IsNullOrWhiteSpace(x.Title))
                .WithMessage("Title must not exceed 200 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(2000)
                .When(x => !string.IsNullOrWhiteSpace(x.Description))
                .WithMessage("Description must not exceed 2000 characters.");

            RuleFor(x => x.Url)
                .Must(BeAValidUrl)
                .When(x => !string.IsNullOrWhiteSpace(x.Url))
                .WithMessage("Url must be a valid absolute URL.");

            RuleFor(x => x.Provider)
                .MaximumLength(200)
                .When(x => !string.IsNullOrWhiteSpace(x.Provider))
                .WithMessage("Provider must not exceed 200 characters.");

            RuleFor(x => x.InstructorName)
                .MaximumLength(200)
                .When(x => !string.IsNullOrWhiteSpace(x.InstructorName))
                .WithMessage("InstructorName must not exceed 200 characters.");

            RuleFor(x => x.EstimatedDurationInHours)
                .GreaterThan(0)
                .When(x => x.EstimatedDurationInHours.HasValue)
                .WithMessage("EstimatedDurationInHours must be greater than 0.");
        }

        private static bool BeAValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }
    }
}
