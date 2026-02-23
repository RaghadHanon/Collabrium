using API.Contracts.Tenants;
using FluentValidation;

namespace API.Validators.Tenants;

public sealed class GetTenantsRequestValidator : AbstractValidator<GetTenantsRequest>
{
    public GetTenantsRequestValidator()
    {
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100.");

        RuleFor(x => x.NameContains)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.NameContains));

        RuleFor(x => x.DomainContains)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.DomainContains));

        RuleFor(x => x.DescriptionContains)
            .MaximumLength(250)
            .When(x => !string.IsNullOrWhiteSpace(x.DescriptionContains));

        RuleFor(x => x.Country)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Country));

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?\d{9,15}$")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithMessage("Phone number must be valid and contain only digits (optionally starting with +).");

        RuleFor(x => x.CreatedBefore)
            .GreaterThanOrEqualTo(x => x.CreatedAfter!.Value)
            .When(x => x.CreatedAfter.HasValue && x.CreatedBefore.HasValue)
            .WithMessage("CreatedBefore must be greater than or equal to CreatedAfter.");
    }
}