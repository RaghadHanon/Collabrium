using API.Contracts.Reports;
using FluentValidation;

namespace API.Validators.Reports;

public sealed class GetOverdueWorkItemsReportRequestValidator
    : ReportDateRangeRequestValidator<GetOverdueWorkItemsReportRequest>
{
    public GetOverdueWorkItemsReportRequestValidator()
    {
        RuleFor(x => x.MaxItems)
            .GreaterThan(0)
            .WithMessage("MaxItems must be greater than zero.");
    }
}