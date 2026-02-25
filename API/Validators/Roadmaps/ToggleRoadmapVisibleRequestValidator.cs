using API.Contracts.Roadmaps;
using FluentValidation;

namespace API.Validators.Roadmaps
{
    public sealed class ToggleRoadmapVisibleRequestValidator
     : AbstractValidator<ToggleRoadmapVisibleRequest>
    {
        public ToggleRoadmapVisibleRequestValidator()
        {
            RuleFor(x => x.RoadmapId)
                .NotEmpty().WithMessage("RoadmapId is required.");
        }
    }
}
