using API.Contracts.Posts;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.Posts
{
    public sealed class UpdatePostRequestValidator : AbstractValidator<UpdatePostRequest>
    {
        public UpdatePostRequestValidator()
        {
            RuleFor(x => x.Title)
                .MaximumLength(200)
                .When(x => !string.IsNullOrWhiteSpace(x.Title))
                .WithMessage("Title must not exceed 200 characters.");

            RuleFor(x => x.Content)
                .MaximumLength(5000)
                .When(x => !string.IsNullOrWhiteSpace(x.Content))
                .WithMessage("Content must not exceed 5000 characters.");

            RuleFor(x => x.Links)
                .Must(x => x == null || x.Count <= 5)
                .WithMessage("A post can have maximum 5 links.");

            RuleForEach(x => x.Links)
                .NotEmpty()
                .WithMessage("Link cannot be empty.")
                .Must(BeAValidUrl)
                .WithMessage("Link must be a valid absolute URL.")
                .When(x => x.Links is not null);

            RuleFor(x => x.Links)
                .Must(x => x == null || x.Distinct().Count() == x.Count)
                .WithMessage("Links must be unique.");

            RuleFor(x => x)
                .Must(x =>
                    x.Type != PostType.Resource
                        ? x.Links is null || x.Links.Count == 0
                        : true)
                .WithMessage("Links are allowed only for resource posts.");
        }

        private static bool BeAValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }
    }
}
