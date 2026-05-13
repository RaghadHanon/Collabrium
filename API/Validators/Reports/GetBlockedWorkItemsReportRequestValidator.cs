using API.Contracts.Reports;
using FluentValidation;

namespace API.Validators.Reports;

public sealed class GetBlockedWorkItemsReportRequestValidator
    : ReportDateRangeRequestValidator<GetBlockedWorkItemsReportRequest>
{
    public GetBlockedWorkItemsReportRequestValidator()
    {
        RuleFor(x => x.MaxItems)
            .GreaterThan(0)
            .WithMessage("MaxItems must be greater than zero.");
    }
}
