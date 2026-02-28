using API.Contracts.WorkItems.Create;
using FluentValidation;

namespace API.Validators.WorkItems.Create;

public sealed class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskRequestValidator()
    {
        Include(new CreateWorkItemRequestValidator());
    }
}