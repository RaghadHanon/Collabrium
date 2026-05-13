using API.Contracts.Articles;
using FluentValidation;

namespace API.Validators.Articles
{
    public sealed class AddArticleCommentRequestValidator : AbstractValidator<AddArticleCommentRequest>
    {
        public AddArticleCommentRequestValidator()
        {
            RuleFor(x => x.Content)
                .NotEmpty()
                .WithMessage("Comment content is required.")
                .MaximumLength(2000)
                .WithMessage("Comment content must not exceed 2000 characters.");
        }
    }
}
