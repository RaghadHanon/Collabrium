using API.Contracts.Projects;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.Projects;

public sealed class GetProjectsRequestValidator : AbstractValidator<GetProjectsRequest>
{
    public GetProjectsRequestValidator()
    {
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100.");

        RuleFor(x => x.TeamId)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .When(x => x.TeamId is not null)
            .WithMessage("TeamId cannot be empty.");

        RuleFor(x => x.Status)
            .Must(status => Enum.IsDefined(typeof(ProjectStatus), status))
            .When(x => !string.IsNullOrWhiteSpace(x.Status))
            .WithMessage("Status must be a valid ProjectStatus value.");

        RuleFor(x => x.NameContains)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.NameContains));

        RuleFor(x => x.StartBefore)
            .GreaterThanOrEqualTo(x => x.StartAfter!.Value)
            .When(x => x.StartAfter.HasValue && x.StartBefore.HasValue)
            .WithMessage("StartBefore must be greater than or equal to StartAfter.");

        RuleFor(x => x.EndBefore)
            .GreaterThanOrEqualTo(x => x.EndAfter!.Value)
            .When(x => x.EndAfter.HasValue && x.EndBefore.HasValue)
            .WithMessage("EndBefore must be greater than or equal to EndAfter.");

        RuleFor(x => x.CreatedBefore)
            .GreaterThanOrEqualTo(x => x.CreatedAfter!.Value)
            .When(x => x.CreatedAfter.HasValue && x.CreatedBefore.HasValue)
            .WithMessage("CreatedBefore must be greater than or equal to CreatedAfter.");
    }
}
