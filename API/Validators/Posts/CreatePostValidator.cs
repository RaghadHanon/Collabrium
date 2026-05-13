using API.Contracts.Posts;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.Posts
{
    public sealed class CreatePostRequestValidator : AbstractValidator<CreatePostRequest>
    {
        public CreatePostRequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Title is required.")
                .MaximumLength(200)
                .WithMessage("Title must not exceed 200 characters.");

            RuleFor(x => x.Content)
                .NotEmpty()
                .WithMessage("Content is required.")
                .MaximumLength(5000)
                .WithMessage("Content must not exceed 5000 characters.");

            RuleFor(x => x.TagIds)
                .Must(x => x.Count <= 5)
                .WithMessage("A post can have maximum 5 tags.");

            RuleForEach(x => x.TagIds)
                .NotEmpty()
                .WithMessage("Tag id cannot be empty.");

            RuleFor(x => x.TagIds)
                .Must(x => x.Distinct().Count() == x.Count)
                .WithMessage("Tag ids must be unique.");

            RuleFor(x => x.Links)
                .Must(x => x.Count <= 5)
                .WithMessage("A post can have maximum 5 links.");

            RuleForEach(x => x.Links)
                .NotEmpty()
                .WithMessage("Link cannot be empty.")
                .Must(BeAValidUrl)
                .WithMessage("Link must be a valid absolute URL.");

            RuleFor(x => x)
                .Must(x => x.Type == PostType.Resource || x.Links.Count == 0)
                .WithMessage("Links are allowed only for resource posts.");

            RuleFor(x => x.Links)
                .Must(x => x.Distinct().Count() == x.Count)
                .WithMessage("Links must be unique.");
        }

        private static bool BeAValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }
    }
}
