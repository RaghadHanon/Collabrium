using API.Contracts.WorkItems.Update;
using FluentValidation;

namespace API.Validators.WorkItems.Update;

public sealed class UpdateEpicRequestValidator : AbstractValidator<UpdateEpicRequest>
{
    public UpdateEpicRequestValidator()
    {
        Include(new UpdateWorkItemRequestValidator());
    }
}
