using API.Contracts.Articles;
using FluentValidation;

namespace API.Validators.Articles
{
    public sealed class GetArticleCommentsRequestValidator : AbstractValidator<GetArticleCommentsRequest>
    {
        public GetArticleCommentsRequestValidator()
        {
            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("PageSize must be between 1 and 100.");
        }
    }
}
