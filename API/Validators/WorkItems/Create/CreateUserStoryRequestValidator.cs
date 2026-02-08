using API.Contracts.WorkItems.Create;
using FluentValidation;

namespace API.Validators.WorkItems.Create;

public sealed class CreateUserStoryRequestValidator : AbstractValidator<CreateUserStoryRequest>
{
    public CreateUserStoryRequestValidator()
    {
        Include(new CreateWorkItemRequestValidator());

        RuleFor(x => x.StoryPoints)
            .GreaterThan(0).WithMessage("StoryPoints must be greater than 0.");
    }
}