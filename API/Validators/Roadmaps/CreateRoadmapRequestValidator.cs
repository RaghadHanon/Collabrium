using API.Contracts.Roadmaps;
using FluentValidation;

namespace API.Validators.Roadmaps;

public sealed class CreateRoadmapRequestValidator
 : AbstractValidator<CreateRoadmapRequest>
{
    public CreateRoadmapRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200)
            .WithMessage("Title cannot exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Description cannot exceed 2000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}
