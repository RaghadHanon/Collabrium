using API.Contracts.WorkItems.Create;
using FluentValidation;

namespace API.Validators.WorkItems.Create;

public sealed class CreateUserStoryRequestValidator : AbstractValidator<CreateUserStoryRequest>
{
    public CreateUserStoryRequestValidator()
    {
        Include(new CreateWorkItemRequestValidator());

        RuleFor(x => x.StoryPoints)
            .GreaterThan(0)
            .Must(sp =>
            {
                var allowedValues = new[] { 1, 2, 3, 5, 8, 13 };
                return allowedValues.Contains(sp);
            })
            .WithMessage("StoryPoints must one of these values [1, 2, 3, 5, 8, 13].");
    }
}