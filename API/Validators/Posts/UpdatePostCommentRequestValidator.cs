using API.Contracts.Posts;
using FluentValidation;

namespace API.Validators.Posts
{
    public sealed class UpdatePostCommentRequestValidator : AbstractValidator<UpdatePostCommentRequest>
    {
        public UpdatePostCommentRequestValidator()
        {
            RuleFor(x => x.Content)
                .NotEmpty()
                .WithMessage("Content is required.")
                .MaximumLength(1000)
                .WithMessage("Content must not exceed 1000 characters.");
        }
    }
}
