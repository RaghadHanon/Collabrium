using API.Contracts.AI;
using FluentValidation;

namespace API.Validators.AI;

public sealed class GenerateTopicSuggestionsRequestValidator
    : AbstractValidator<GenerateTopicSuggestionsRequest>
{
    public GenerateTopicSuggestionsRequestValidator()
    {
        RuleFor(x => x.SuggestionsCount)
            .InclusiveBetween(1, 5);
    }
}