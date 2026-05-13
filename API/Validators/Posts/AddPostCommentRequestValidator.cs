using API.Contracts.Posts;
using FluentValidation;

namespace API.Validators.Posts
{
    public sealed class AddPostCommentRequestValidator : AbstractValidator<AddPostCommentRequest>
    {
        public AddPostCommentRequestValidator()
        {
            RuleFor(x => x.Content)
                .NotEmpty()
                .WithMessage("Content is required.")
                .MaximumLength(1000)
                .WithMessage("Content must not exceed 1000 characters.");
        }
    }
}
