using API.Contracts.Sprints;
using Domain.Enums;
using FluentValidation;

namespace API.Validators.Sprints;

public sealed class GetSprintsRequestValidator : AbstractValidator<GetSprintsRequest>
{
    public GetSprintsRequestValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("ProjectId is required.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");

        RuleFor(x => x.Status)
            .Must(status => string.IsNullOrEmpty(status) || Enum.TryParse<SprintStatus>(status, true, out _))
            .WithMessage("Status must be a valid SprintStatus value.");

        When(x => x.StartAfter.HasValue && x.StartBefore.HasValue, () =>
        {
            RuleFor(x => x.StartBefore!.Value)
                .GreaterThanOrEqualTo(x => x.StartAfter!.Value)
                .WithMessage("StartBefore must be >= StartAfter.");
        });

        When(x => x.EndAfter.HasValue && x.EndBefore.HasValue, () =>
        {
            RuleFor(x => x.EndBefore!.Value)
                .GreaterThanOrEqualTo(x => x.EndAfter!.Value)
                .WithMessage("EndBefore must be >= EndAfter.");
        });

        When(x => x.CreatedAfter.HasValue && x.CreatedBefore.HasValue, () =>
        {
            RuleFor(x => x.CreatedBefore!.Value)
                .GreaterThanOrEqualTo(x => x.CreatedAfter!.Value)
                .WithMessage("CreatedBefore must be >= CreatedAfter.");
        });
    }
}