using API.Contracts.AI;
using FluentValidation;

namespace API.Validators.AI;

public sealed class GenerateSubTopicSuggestionsRequestValidator
    : AbstractValidator<GenerateSubTopicSuggestionsRequest>
{
    public GenerateSubTopicSuggestionsRequestValidator()
    {
        RuleFor(x => x.SuggestionsCount)
            .InclusiveBetween(1, 5);
    }
}