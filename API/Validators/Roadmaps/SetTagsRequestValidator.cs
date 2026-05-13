using API.Contracts.Roadmaps;
using FluentValidation;

namespace API.Validators.Roadmaps;

public sealed class SetTagsRequestValidator : AbstractValidator<SetTagsRequest>
{
    public SetTagsRequestValidator()
    {
        RuleFor(x => x.RoadmapId)
            .NotEmpty().WithMessage("RoadmapId is required.");

        RuleFor(x => x.TagIds)
            .NotNull().WithMessage("TagIds are required.");

        RuleForEach(x => x.TagIds)
            .Must(id => !string.IsNullOrWhiteSpace(id))
            .WithMessage("TagId cannot be empty.");
    }
}
