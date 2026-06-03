using API.Contracts.Roadmaps;
using FluentValidation;

namespace API.Validators.Roadmaps;

public sealed class UpdateRoadmapRequestValidator
 : AbstractValidator<UpdateRoadmapRequest>
{
    public UpdateRoadmapRequestValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Title));

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.PublishedAt)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .When(x => x.PublishedAt.HasValue)
            .WithMessage("PublishedAt cannot be in the future.");
    }
}
