using API.Contracts.Articles;
using FluentValidation;

namespace API.Validators.Articles
{
    public sealed class UpdateArticleRequestValidator : AbstractValidator<UpdateArticleRequest>
    {
        public UpdateArticleRequestValidator()
        {
            RuleFor(x => x.Title)
                .MaximumLength(300)
                .When(x => !string.IsNullOrWhiteSpace(x.Title))
                .WithMessage("Title must not exceed 300 characters.");

            RuleFor(x => x.Title)
                .Must(x => x is null || !string.IsNullOrWhiteSpace(x))
                .WithMessage("Title cannot be empty.");

            RuleFor(x => x.Content)
                .Must(x => x is null || !string.IsNullOrWhiteSpace(x))
                .WithMessage("Content cannot be empty.");

            RuleFor(x => x)
                .Must(x =>
                    x.Title is not null ||
                    x.Content is not null ||
                    x.IsPublished.HasValue)
                .WithMessage("At least one field must be provided.");
        }
    }
}
