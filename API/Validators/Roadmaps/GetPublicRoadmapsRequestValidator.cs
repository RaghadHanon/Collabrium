using API.Contracts.Roadmaps;
using FluentValidation;

namespace API.Validators.Roadmaps
{
    public sealed class GetPublicRoadmapsRequestValidator
     : AbstractValidator<GetPublicRoadmapsRequest>
    {
        public GetPublicRoadmapsRequestValidator()
        {
            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("PageSize must be between 1 and 100.");

            RuleFor(x => x.TitleContains)
                .MaximumLength(200)
                .When(x => !string.IsNullOrWhiteSpace(x.TitleContains));

            RuleFor(x => x.DescriptionContains)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.DescriptionContains));

            RuleFor(x => x.CreatedByUserId)
                .Must(x => !string.IsNullOrWhiteSpace(x))
                .When(x => x.CreatedByUserId is not null)
                .WithMessage("CreatedByUserId cannot be empty.");

            RuleFor(x => x.PublishedBefore)
                .GreaterThanOrEqualTo(x => x.PublishedAfter!.Value)
                .When(x => x.PublishedAfter.HasValue && x.PublishedBefore.HasValue)
                .WithMessage("PublishedBefore must be greater than or equal to PublishedAfter.");

            RuleFor(x => x.UpdatedBefore)
                .GreaterThanOrEqualTo(x => x.UpdatedAfter!.Value)
                .When(x => x.UpdatedAfter.HasValue && x.UpdatedBefore.HasValue);

            RuleFor(x => x.CreatedBefore)
                .GreaterThanOrEqualTo(x => x.CreatedAfter!.Value)
                .When(x => x.CreatedAfter.HasValue && x.CreatedBefore.HasValue);
        }
    }
}
