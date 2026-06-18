using API.Contracts.AI;
using FluentValidation;

namespace API.Validators.AI;

public sealed class GenerateRoadmapFullDraftRequestValidator
    : AbstractValidator<GenerateRoadmapFullDraftRequest>
{
    public GenerateRoadmapFullDraftRequestValidator()
    {
        RuleFor(x => x.Goal)
            .NotEmpty()
            .MaximumLength(1250);

        RuleFor(x => x.TopicsCount)
            .InclusiveBetween(3, 10);

        RuleFor(x => x.SubTopicsPerTopic)
            .InclusiveBetween(1, 6);

    }
}