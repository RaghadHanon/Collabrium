using API.Contracts.Teams;
using FluentValidation;

namespace API.Validators.Teams;

public sealed class GetTeamsRequestValidator : AbstractValidator<GetTeamsRequest>
{
    public GetTeamsRequestValidator()
    {
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100.");

        RuleFor(x => x.TeamLeadName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.TeamLeadName));

        RuleFor(x => x.NameContains)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.NameContains));

        RuleFor(x => x.DescriptionContains)
            .MaximumLength(250)
            .When(x => !string.IsNullOrWhiteSpace(x.DescriptionContains));

        RuleFor(x => x.CapacityMin)
            .GreaterThanOrEqualTo(1)
            .When(x => x.CapacityMin.HasValue)
            .WithMessage("CapacityMin must be >= 1.");

        RuleFor(x => x.CapacityMax)
            .GreaterThanOrEqualTo(1)
            .When(x => x.CapacityMax.HasValue)
            .WithMessage("CapacityMax must be >= 1.");

        RuleFor(x => x)
            .Must(x => !x.CapacityMin.HasValue || !x.CapacityMax.HasValue || x.CapacityMax >= x.CapacityMin)
            .WithMessage("CapacityMax must be greater than or equal to CapacityMin.");

        RuleFor(x => x.UpdatedBefore)
            .GreaterThanOrEqualTo(x => x.UpdatedAfter!.Value)
            .When(x => x.UpdatedAfter.HasValue && x.UpdatedBefore.HasValue)
            .WithMessage("UpdatedBefore must be greater than or equal to UpdatedAfter.");

        RuleFor(x => x.CreatedBefore)
            .GreaterThanOrEqualTo(x => x.CreatedAfter!.Value)
            .When(x => x.CreatedAfter.HasValue && x.CreatedBefore.HasValue)
            .WithMessage("CreatedBefore must be greater than or equal to CreatedAfter.");
    }
}
