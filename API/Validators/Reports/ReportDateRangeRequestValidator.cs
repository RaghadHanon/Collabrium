using API.Contracts.Reports;
using FluentValidation;

namespace API.Validators.Reports;

public class ReportDateRangeRequestValidator<TRequest> : AbstractValidator<TRequest>
    where TRequest : ReportDateRangeRequest
{
    public ReportDateRangeRequestValidator()
    {
        RuleFor(x => x.To)
            .GreaterThanOrEqualTo(x => x.From)
            .WithMessage("To date must be greater than or equal to From date.")
            .Must(to => to <= DateTime.UtcNow.AddHours(12))
            .WithMessage("To date cannot be in the future.");
    }
}