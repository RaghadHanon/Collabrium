using API.Contracts.Articles;
using FluentValidation;

namespace API.Validators.Articles
{
    public sealed class SetArticleTagsRequestValidator : AbstractValidator<SetArticleTagsRequest>
    {
        public SetArticleTagsRequestValidator()
        {
            RuleForEach(x => x.TagIds)
                .NotEmpty()
                .WithMessage("Tag id must not be empty.");
        }
    }
}
