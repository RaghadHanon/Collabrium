using API.Contracts.Teams;
using FluentValidation;

namespace API.Validators.Teams;

public sealed class AddMembersToTeamRequestValidator : AbstractValidator<AddMembersToTeamRequest>
{
    public AddMembersToTeamRequestValidator()
    {
        RuleFor(x => x.TeamId)
            .NotEmpty().WithMessage("TeamId is required.");

        RuleFor(x => x.MembersIds)
            .NotNull().WithMessage("MembersIds is required.")
            .Must(x => x.Length > 0).WithMessage("At least one member id is required.");

        RuleForEach(x => x.MembersIds)
            .NotEmpty().WithMessage("Member id cannot be empty.")
            .Must(id => !string.IsNullOrWhiteSpace(id))
            .WithMessage("Member id cannot be whitespace.");

        RuleFor(x => x.MembersIds)
            .Must(ids => ids.Where(i => !string.IsNullOrWhiteSpace(i))
                            .Select(i => i.Trim())
                            .Distinct(StringComparer.Ordinal)
                            .Count() == ids.Where(i => !string.IsNullOrWhiteSpace(i)).Count())
            .When(x => x.MembersIds is not null && x.MembersIds.Length > 0)
            .WithMessage("MembersIds contains duplicates.");
    }
}