using API.Contracts.Articles;
using FluentValidation;

namespace API.Validators.Articles
{
    public sealed class CreateArticleRequestValidator : AbstractValidator<CreateArticleRequest>
    {
        public CreateArticleRequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(300).WithMessage("Title must not exceed 300 characters.");
            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Content is required.");


        }
    }
}
