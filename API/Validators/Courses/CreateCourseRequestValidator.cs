using API.Contracts.Courses;
using FluentValidation;

namespace API.Validators.Courses
{
    public sealed class CreateCourseRequestValidator : AbstractValidator<CreateCourseRequest>
    {
        public CreateCourseRequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Title is required.")
                .MaximumLength(200)
                .WithMessage("Title must not exceed 200 characters.");

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessage("Description is required.")
                .MaximumLength(2000)
                .WithMessage("Description must not exceed 2000 characters.");

            RuleFor(x => x.Url)
                .NotEmpty()
                .WithMessage("Url is required.")
                .Must(BeAValidUrl)
                .WithMessage("Url must be a valid absolute URL.");

            RuleFor(x => x.Provider)
                .NotEmpty()
                .WithMessage("Provider is required.")
                .MaximumLength(200)
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
