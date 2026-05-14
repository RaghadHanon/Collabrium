using API.Contracts.Reports;
using FluentValidation;

namespace API.Validators.Reports;

public sealed class GetDeveloperProductivityReportRequestValidator
    : ReportDateRangeRequestValidator<GetDeveloperProductivityReportRequest>
{
    public GetDeveloperProductivityReportRequestValidator()
    {
        RuleFor(x => x)
            .Must(x => string.IsNullOrWhiteSpace(x.TeamId) || string.IsNullOrWhiteSpace(x.UserId))
            .WithMessage("Choose either TeamId or UserId, not both.");
    }
}