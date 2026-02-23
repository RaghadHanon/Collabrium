using API.Contracts.WorkItems.Comments;
using FluentValidation;

namespace API.Validators.WorkItems.Comments;

public class UpdateWorkItemCommentRequestValidator : AbstractValidator<UpdateWorkItemCommentRequest>
{
    public UpdateWorkItemCommentRequestValidator()
    {
        RuleFor(x => x.NewContent)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("Content can't be empty or null");
    }
}