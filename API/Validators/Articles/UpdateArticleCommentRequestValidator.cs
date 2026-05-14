using API.Contracts.Articles;
using FluentValidation;

namespace API.Validators.Articles
{
    public sealed class UpdateArticleCommentRequestValidator : AbstractValidator<UpdateArticleCommentRequest>
    {
        public UpdateArticleCommentRequestValidator()
        {
            RuleFor(x => x.Content)
                .NotEmpty()
                .WithMessage("Comment content is required.")
                .MaximumLength(2000)
                .WithMessage("Comment content must not exceed 2000 characters.");
        }
    }
}
