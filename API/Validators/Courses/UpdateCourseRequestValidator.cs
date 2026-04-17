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
            RuleFor(x => x.LearningPoints)
                .Must(lp => lp == null || lp.Count <= 20)
                .WithMessage("You can add up to 20 learning points only.");

            RuleForEach(x => x.LearningPoints)
                .NotEmpty().WithMessage("Learning point cannot be empty.")
                .Must(lp => !string.IsNullOrWhiteSpace(lp))
                .WithMessage("Learning point cannot be whitespace.")
                .MaximumLength(200)
                .WithMessage("Learning point must not exceed 200 characters.")
                .When(x => x.LearningPoints != null);

            RuleFor(x => x.LearningPoints)
                .Must(lp => lp == null || lp
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim().ToLower())
                    .Distinct()
                    .Count() == lp.Count)
                .WithMessage("Learning points must be unique.");
        }

        private static bool BeAValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }
    }
}
