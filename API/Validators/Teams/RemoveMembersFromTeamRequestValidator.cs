using API.Contracts.Teams;
using FluentValidation;

namespace API.Validators.Teams;

public sealed class RemoveMembersFromTeamRequestValidator : AbstractValidator<RemoveMembersFromTeamRequest>
{
    public RemoveMembersFromTeamRequestValidator()
    {
        RuleFor(x => x.TeamId)
            .NotEmpty().WithMessage("TeamId is required.");

        RuleFor(x => x.MemberIds)
            .NotNull().WithMessage("MemberIds is required.")
            .Must(x => x.Length > 0).WithMessage("At least one member id is required.");

        RuleForEach(x => x.MemberIds)
            .NotEmpty().WithMessage("Member id cannot be empty.")
            .Must(id => !string.IsNullOrWhiteSpace(id))
            .WithMessage("Member id cannot be whitespace.");

        RuleFor(x => x.MemberIds)
            .Must(ids => ids.Where(i => !string.IsNullOrWhiteSpace(i))
                            .Select(i => i.Trim())
                            .Distinct(StringComparer.Ordinal)
                            .Count() == ids.Where(i => !string.IsNullOrWhiteSpace(i)).Count())
            .When(x => x.MemberIds is not null && x.MemberIds.Length > 0)
            .WithMessage("MemberIds contains duplicates.");
    }
}
