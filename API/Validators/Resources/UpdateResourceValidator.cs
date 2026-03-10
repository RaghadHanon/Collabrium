using Application.Features.RoadmapResources.Commands;
using FluentValidation;

namespace API.Validators.Resources
{
    public sealed class UpdateResourceValidator
        : AbstractValidator<UpdateResource.Command>
    {
        public UpdateResourceValidator()
        {
            RuleFor(x => x.RoadmapId)
                .NotEmpty()
                .WithMessage("RoadmapId is required.");

            RuleFor(x => x.TopicNodeId)
                .NotEmpty()
                .WithMessage("TopicNodeId is required.");

            RuleFor(x => x.SubTopicId)
                .NotEmpty()
                .WithMessage("SubTopicId is required.");

            RuleFor(x => x.ResourceId)
                .NotEmpty()
                .WithMessage("ResourceId is required.");

            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Title is required.")
                .MaximumLength(200)
                .WithMessage("Title must not exceed 200 characters.");

            RuleFor(x => x.Url)
                .NotEmpty()
                .WithMessage("Url is required.")
                .Must(BeAValidUrl)
                .WithMessage("Url must be a valid absolute URL.");

            RuleFor(x => x.Order)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Order must be greater than or equal to 0.");

            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage("Invalid resource type.");
        }

        private static bool BeAValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }
    }
}
