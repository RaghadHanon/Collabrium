using API.Contracts.AI;
using FluentValidation;

namespace API.Validators.AI;

public sealed class GenerateResourceSuggestionsRequestValidator
    : AbstractValidator<GenerateResourceSuggestionsRequest>
{
    public GenerateResourceSuggestionsRequestValidator()
    {
        RuleFor(x => x.SuggestionsCount)
            .InclusiveBetween(1, 5);
    }
}