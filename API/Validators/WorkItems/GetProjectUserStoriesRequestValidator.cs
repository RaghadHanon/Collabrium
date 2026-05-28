using API.Contracts.WorkItems;
using FluentValidation;

namespace API.Validators.WorkItems;

public sealed class GetProjectUserStoriesRequestValidator
    : AbstractValidator<GetProjectWorkItemByTypeRequest>
{
    public GetProjectUserStoriesRequestValidator()
    {
        Include(new GetSprintUserStoriesRequestValidator());

        RuleFor(x => x.SprintId)
            .NotEmpty()
            .When(x => x.SprintId is not null)
            .WithMessage("SprintId cannot be empty.");

        RuleFor(x => x.SprintId)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.SprintId))
            .WithMessage("SprintId cannot exceed 50 characters.");
    }
}