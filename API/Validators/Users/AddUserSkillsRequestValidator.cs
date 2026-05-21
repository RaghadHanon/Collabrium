using API.Contracts.Users;
using FluentValidation;

namespace API.Validators.Users;

public sealed class AddUserSkillsRequestValidator : AbstractValidator<AddUserSkillsRequest>
{
    public AddUserSkillsRequestValidator()
    {
        RuleFor(x => x.SkillIds)
            .NotNull()
            .WithMessage("SkillIds are required.")
            .Must(x => x.Count > 0)
            .WithMessage("At least one skill is required.");

        RuleForEach(x => x.SkillIds)
            .Must(s => string.IsNullOrEmpty(s))
            .WithMessage("SkillId can't be null or empty.");
    }
}