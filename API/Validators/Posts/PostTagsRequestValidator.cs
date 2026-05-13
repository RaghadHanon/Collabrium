using API.Contracts.Posts;
using FluentValidation;

namespace API.Validators.Posts
{
    public sealed class PostTagsRequestValidator : AbstractValidator<PostTagsRequest>
    {
        public PostTagsRequestValidator()
        {
            RuleFor(x => x.TagIds)
                .NotNull()
                .WithMessage("TagIds are required.")
                .Must(x => x.Count <= 5)
                .WithMessage("A post can have maximum 5 tags.");

            RuleForEach(x => x.TagIds)
                .NotEmpty()
                .WithMessage("Tag id cannot be empty.");

            RuleFor(x => x.TagIds)
                .Must(x => x.Distinct().Count() == x.Count)
                .WithMessage("Tag ids must be unique.");
        }
    }
}
