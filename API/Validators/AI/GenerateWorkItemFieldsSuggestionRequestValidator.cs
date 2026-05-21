using API.Contracts.AI.WorkItems;
using FluentValidation;

namespace API.Validators.AI.WorkItems;

public sealed class GenerateWorkItemFieldsSuggestionRequestValidator
    : AbstractValidator<GenerateWorkItemFieldsSuggestionRequest>
{
    public GenerateWorkItemFieldsSuggestionRequestValidator()
    {
        RuleFor(x => x.Summary)
            .NotEmpty()
            .MaximumLength(1250);
    }
}