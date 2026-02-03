using API.Contracts.Tenants;
using FluentValidation;

namespace API.Validators.Tenants;

public sealed class GetTenantUsersRequestValidator : AbstractValidator<GetTenantUsersRequest>
{
    public GetTenantUsersRequestValidator()
    {
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100.");

        RuleFor(x => x.NameContains)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.NameContains));

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?\d{9,15}$")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithMessage("Phone number must be valid and contain only digits (optionally starting with +).");

        RuleFor(x => x.CreatedBefore)
            .GreaterThanOrEqualTo(x => x.CreatedAfter!.Value)
            .When(x => x.CreatedAfter.HasValue && x.CreatedBefore.HasValue)
            .WithMessage("CreatedBefore must be greater than or equal to CreatedAfter.");

        RuleFor(x => x.LastLoginAfter)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .When(x => x.LastLoginAfter.HasValue)
            .WithMessage("LastLoginAfter cannot be in the future.");

        RuleFor(x => x.DateOfBirth)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-18))
            .When(x => x.DateOfBirth.HasValue)
            .WithMessage("User must be at least 18 years old.");
    }
}