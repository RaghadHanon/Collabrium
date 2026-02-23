using API.Contracts.WorkItems.Create;
using FluentValidation;

namespace API.Validators.WorkItems.Create;

public sealed class CreateEpicRequestValidator : AbstractValidator<CreateEpicRequest>
{
    public CreateEpicRequestValidator()
    {
        Include(new CreateWorkItemRequestValidator());
    }
}
