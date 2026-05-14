using API.Contracts.Articles;
using FluentValidation;

namespace API.Validators.Articles
{
    public sealed class GetArticlesRequestValidator : AbstractValidator<GetArticlesRequest>
    {
        public GetArticlesRequestValidator()
        {
            RuleFor(x => x.Search)
                .MaximumLength(300)
                .When(x => !string.IsNullOrWhiteSpace(x.Search))
                .WithMessage("Search must not exceed 300 characters.");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("PageSize must be between 1 and 100.");

            RuleForEach(x => x.TagIdsAny)
                .NotEmpty()
                .When(x => x.TagIdsAny is not null)
                .WithMessage("Tag id must not be empty.");

            RuleForEach(x => x.TagIdsAll)
                .NotEmpty()
                .When(x => x.TagIdsAll is not null)
                .WithMessage("Tag id must not be empty.");

            RuleFor(x => x)
                .Must(x => !x.CreatedAfter.HasValue || !x.CreatedBefore.HasValue || x.CreatedAfter <= x.CreatedBefore)
                .WithMessage("CreatedAfter must be before or equal to CreatedBefore.");

            RuleFor(x => x)
                .Must(x => !x.UpdatedAfter.HasValue || !x.UpdatedBefore.HasValue || x.UpdatedAfter <= x.UpdatedBefore)
                .WithMessage("UpdatedAfter must be before or equal to UpdatedBefore.");
        }
    }
}
