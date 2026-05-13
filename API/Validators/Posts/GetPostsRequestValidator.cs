using API.Contracts.Posts;
using FluentValidation;

namespace API.Validators.Posts
{
    public sealed class GetPostsRequestValidator : AbstractValidator<GetPostsRequest>
    {
        public GetPostsRequestValidator()
        {
            RuleFor(x => x.Search)
                .MaximumLength(200)
                .When(x => !string.IsNullOrWhiteSpace(x.Search))
                .WithMessage("Search must not exceed 200 characters.");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 50)
                .WithMessage("PageSize must be between 1 and 50.");

            RuleFor(x => x.TagIdsAny)
                .Must(x => x == null || x.Count <= 10)
                .WithMessage("TagIdsAny can contain maximum 10 items.");

            RuleFor(x => x.TagIdsAll)
                .Must(x => x == null || x.Count <= 10)
                .WithMessage("TagIdsAll can contain maximum 10 items.");

            RuleFor(x => x)
                .Must(x =>
                    !x.CreatedAfter.HasValue ||
                    !x.CreatedBefore.HasValue ||
                    x.CreatedAfter <= x.CreatedBefore)
                .WithMessage("CreatedAfter must be earlier than CreatedBefore.");

            RuleFor(x => x)
                .Must(x =>
                    !x.UpdatedAfter.HasValue ||
                    !x.UpdatedBefore.HasValue ||
                    x.UpdatedAfter <= x.UpdatedBefore)
                .WithMessage("UpdatedAfter must be earlier than UpdatedBefore.");
        }
    }
}
