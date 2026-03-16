using API.Contracts.ProjectHistory;
using Domain.Enums;
using Domain.Models;
using FluentValidation;

namespace API.Validators.ProjectHistory;

public sealed class GetProjectHistoryRequestValidator : AbstractValidator<GetProjectHistoryRequest>
{
    public GetProjectHistoryRequestValidator()
    {
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100.");

        RuleFor(x => x.MadeByUserId)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .When(x => x.MadeByUserId is not null)
            .WithMessage("MadeByUserId cannot be empty.");

        RuleFor(x => x.Scope)
            .Must(s => Enum.TryParse<ProjectHistoryScope>(s, true, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.Scope))
            .WithMessage("Scope must be a valid ProjectHistoryScope value.");

        RuleFor(x => x.ActionType)
            .Must(s => Enum.TryParse<ActionType>(s, true, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.ActionType))
            .WithMessage("Action Type must be a valid ActionType value.");

        RuleFor(x => x.PropertyName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.PropertyName));

        RuleFor(x => x.RelatedEntityTitle)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.RelatedEntityTitle));

        RuleFor(x => x.CreatedBefore)
            .GreaterThanOrEqualTo(x => x.CreatedAfter!.Value)
            .When(x => x.CreatedAfter.HasValue && x.CreatedBefore.HasValue)
            .WithMessage("CreatedBefore must be greater than or equal to CreatedAfter.");
    }
}
