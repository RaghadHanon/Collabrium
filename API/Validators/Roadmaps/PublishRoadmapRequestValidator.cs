using API.Contracts.Roadmaps;
using FluentValidation;

namespace API.Validators.Roadmaps;

public sealed class PublishRoadmapRequestValidator : AbstractValidator<PublishRoadmapRequest>
{
    public PublishRoadmapRequestValidator()
    {
        RuleFor(x => x.RoadmapId)
            .NotEmpty().WithMessage("RoadmapId is required.");
    }
}
