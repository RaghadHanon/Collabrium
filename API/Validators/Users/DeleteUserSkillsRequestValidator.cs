using API.Contracts.Users;
using FluentValidation;

namespace API.Validators.Users;

public sealed class DeleteUserSkillsRequestValidator : AbstractValidator<DeleteUserSkillsRequest>
{
    public DeleteUserSkillsRequestValidator()
    {
        RuleFor(x => x.SkillIds)
            .NotNull()
            .WithMessage("SkillIds are required.")
            .Must(x => x.Count > 0)
            .WithMessage("At least one skill is required.");

        RuleForEach(x => x.SkillIds)
            .NotEmpty()
            .WithMessage("SkillId can't be null or empty.");
    }
}