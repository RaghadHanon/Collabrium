using API.Contracts.Reports;
using FluentValidation;

namespace API.Validators.Reports;

public sealed class GetCycleTimeReportRequestValidator : AbstractValidator<GetCycleTimeReportRequest>
{
    public GetCycleTimeReportRequestValidator()
    {
        RuleFor(x => x.To)
            .GreaterThanOrEqualTo(x => x.From!.Value)
            .When(x => x.From.HasValue && x.To.HasValue)
            .WithMessage("To date must be greater than or equal to From date.")
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("To date cannot be in the future.");
    }
}