using API.Contracts.WorkItems.Comments;
using FluentValidation;

namespace API.Validators.WorkItems.Relations;

public class AddWorkItemCommentRequestValidator : AbstractValidator<AddWorkItemCommentRequest>
{
    public AddWorkItemCommentRequestValidator()
    {
        RuleFor(x => x.Content)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("Content can't be empty or null");
    }
}
