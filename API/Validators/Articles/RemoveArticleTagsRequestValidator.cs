using API.Contracts.Articles;
using FluentValidation;

namespace API.Validators.Articles
{
    public sealed class RemoveArticleTagsRequestValidator : AbstractValidator<RemoveArticleTagsRequest>
    {
        public RemoveArticleTagsRequestValidator()
        {
            RuleFor(x => x.TagIds)
                .NotEmpty()
                .WithMessage("At least one tag id is required.");

            RuleForEach(x => x.TagIds)
                .NotEmpty()
                .WithMessage("Tag id must not be empty.");
        }
    }
}
