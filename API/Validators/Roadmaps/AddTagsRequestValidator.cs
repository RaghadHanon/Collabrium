using API.Contracts.Roadmaps;
using FluentValidation;

namespace API.Validators.Roadmaps
{
    public sealed class AddTagsRequestValidator : AbstractValidator<AddTagsRequest>
    {
        public AddTagsRequestValidator()
        {
            RuleFor(x => x.RoadmapId)
                .NotEmpty().WithMessage("RoadmapId is required.");

            RuleFor(x => x.TagIds)
                .NotNull().WithMessage("TagIds are required.")
                .Must(x => x.Length > 0)
                .WithMessage("At least one TagId must be provided.");

            RuleForEach(x => x.TagIds)
                .Must(id => !string.IsNullOrWhiteSpace(id))
                .WithMessage("TagId cannot be empty.");
        }
    }
}
