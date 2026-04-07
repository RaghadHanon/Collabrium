using Application.Features.Reports.DTOs;
using Infrastructure.Reports.Common;
using Infrastructure.Reports.Common.PdfComponents;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Infrastructure.Reports.TenantInfo;

public class TenantInfoReportDocument : IDocument
{
    private readonly TenantInfoReport _data;
    private readonly byte[] _mainChart;
    private readonly byte[] _projectsChart;
    private readonly byte[] _sprintsChart;
    private readonly byte[]? _logoBytes;

    public TenantInfoReportDocument(
        TenantInfoReport data,
        byte[] mainChart,
        byte[] projectsChart,
        byte[] sprintsChart,
        byte[]? logoBytes = null)
    {
        _data = data;
        _mainChart = mainChart;
        _projectsChart = projectsChart;
        _sprintsChart = sprintsChart;
        _logoBytes = logoBytes;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        // QuestPDF.Settings.EnableDebugging = true;

        container.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(24);
            page.PageColor(ReportPalette.White);

            page.DefaultTextStyle(ReportTextStyles.Base);

            page.Header().Element(BuildHeader);
            page.Content().PaddingTop(18).Element(BuildContent);
            page.Footer().Element(BuildFooter);
        });
    }

    private void BuildHeader(IContainer container)
    {
        ReportLayoutHelpers.BuildStandardHeader(
            container,
            title: "Tenant Information Report",
            subtitle: $"Portfolio and workspace summary for {_data.Tenant.Name}",
            rightTop: _data.Tenant.Domain,
            rightBottom: $"Generated: {_data.GeneratedAt:dd MMM yyyy}",
            logoBytes: _logoBytes);
    }

    private void BuildContent(IContainer container)
    {
        container.Column(col =>
        {
            col.Spacing(18);

            // Page 1: Profile + Executive Summary
            col.Item().Text("Tenant Profile")
                .Style(ReportTextStyles.SectionTitle);

            col.Item().Element(BuildHeroCard);

            col.Item().Element(BuildContactAndPresenceCard);

            col.Item().Text("Executive Summary")
                .Style(ReportTextStyles.SectionTitle);

            col.Item().Element(BuildKpiRow);

            col.Item().Element(BuildHighlightsCard);

            col.Item().PageBreak();

            // Page 2: Charts
            col.Item().Text("Analytics & Charts")
                .Style(ReportTextStyles.SectionTitle);

            col.Item().Component(new SectionCard(
                title: "Tenant Overview",
                subtitle: "High-level counts across teams, projects, sprints, users, and work items",
                image: _mainChart));

            col.Item().Component(new SectionCard(
                title: "Projects by Status",
                subtitle: "Distribution of projects across lifecycle states",
                image: _projectsChart));

            col.Item().Component(new SectionCard(
                title: "Sprints by Status",
                subtitle: "Distribution of sprints across lifecycle states",
                image: _sprintsChart));

            col.Item().PageBreak();

            // Page 3+: Detailed summaries stacked vertically
            col.Item().Text("Detailed Summaries")
                .Style(ReportTextStyles.SectionTitle);

            col.Item().Element(BuildTenantProfileCard);

            col.Item().Element(BuildOrganizationSummaryCard);

            col.Item().Element(BuildWorkItemsSummaryCard);

            col.Item().Element(BuildProjectAndSprintHealthCard);
        });
    }

    private void BuildHeroCard(IContainer container)
    {
        var totalWorkItems = GetTotalWorkItems();
        var tenantStatus = _data.Tenant.IsActive ? "Active" : "Inactive";
        var description = string.IsNullOrWhiteSpace(_data.Tenant.Description)
            ? "No tenant description is available."
            : _data.Tenant.Description!.Trim();

        container.ApplySoftCard().Column(col =>
        {
            col.Spacing(10);

            col.Item().Text("Tenant Portfolio Snapshot")
                .Style(ReportTextStyles.CardTitle)
                .FontColor(ReportPalette.Primary);

            col.Item().Text(_data.Tenant.Name)
                .FontSize(30)
                .Bold()
                .FontColor(ReportPalette.Ink);

            col.Item().Text(
                $"Tenant {_data.Tenant.Name} ({_data.Tenant.Domain}) is currently {tenantStatus}. " +
                $"It includes {_data.TeamsSummary.TotalTeamsCount:N0} teams, {_data.ProjectsSummary.TotalProjectsCount:N0} projects, " +
                $"{_data.SprintsSummary.TotalSprintsCount:N0} sprints, {_data.UsersSummary.TotalUsersCount:N0} users, " +
                $"and {totalWorkItems:N0} tracked work items across all categories.")
                .Style(ReportTextStyles.Body);

            col.Item().Text(description)
                .Style(ReportTextStyles.Body);

            col.Item().Row(row =>
            {
                row.Spacing(8);
                row.AutoItem().Component(new PillBadge("Portfolio", ReportPalette.PrimarySoft, ReportPalette.Primary));
                row.AutoItem().Component(new PillBadge("Workspace", ReportPalette.AccentSoft, ReportPalette.Accent));
                row.AutoItem().Component(new PillBadge(tenantStatus, ReportPalette.WarningSoft, ReportPalette.Warning));
            });
        });
    }

    private void BuildContactAndPresenceCard(IContainer container)
    {
        var socials = _data.Tenant.ContactDetails.Socials;

        container.ApplyPageCard().Column(col =>
        {
            col.Spacing(10);

            col.Item().Text("Contact & Presence")
                .Style(ReportTextStyles.CardTitle);

            col.Item().Component(new InsightRow("Email", DisplayOrDash(_data.Tenant.ContactDetails.Email)));
            col.Item().Component(new InsightRow("Phone", DisplayOrDash(_data.Tenant.ContactDetails.PhoneNumber)));
            col.Item().Component(new InsightRow("Website", DisplayOrDash(_data.Tenant.WebsiteUrl)));
            col.Item().Component(new InsightRow("Country", DisplayOrDash(_data.Tenant.Country)));
            col.Item().Component(new InsightRow("LinkedIn", DisplayOrDash(socials.LinkedIn)));
            col.Item().Component(new InsightRow("Facebook", DisplayOrDash(socials.Facebook)));
            col.Item().Component(new InsightRow("Instagram", DisplayOrDash(socials.Instagram)));
            col.Item().Component(new InsightRow("YouTube", DisplayOrDash(socials.YouTube)));
            col.Item().Component(new InsightRow("GitHub", DisplayOrDash(socials.Github)));
        });
    }

    private void BuildHighlightsCard(IContainer container)
    {
        var totalWorkItems = GetTotalWorkItems();
        var activeProjects = _data.ProjectsSummary.InProgressProjectsCount;
        var activeSprints = _data.SprintsSummary.ActiveSprintsCount;

        container.ApplyPageCard().Column(col =>
        {
            col.Spacing(10);

            col.Item().Text("Highlights")
                .Style(ReportTextStyles.CardTitle);

            col.Item().Component(new InsightRow("Tenant", _data.Tenant.Name));
            col.Item().Component(new InsightRow("Domain", _data.Tenant.Domain));
            col.Item().Component(new InsightRow("Status", _data.Tenant.IsActive ? "Active" : "Inactive"));
            col.Item().Component(new InsightRow("Projects", $"{_data.ProjectsSummary.TotalProjectsCount:N0}"));
            col.Item().Component(new InsightRow("Active Projects", $"{activeProjects:N0}"));
            col.Item().Component(new InsightRow("Sprints", $"{_data.SprintsSummary.TotalSprintsCount:N0}"));
            col.Item().Component(new InsightRow("Active Sprints", $"{activeSprints:N0}"));
            col.Item().Component(new InsightRow("Users", $"{_data.UsersSummary.TotalUsersCount:N0}"));
            col.Item().Component(new InsightRow("Work Items", $"{totalWorkItems:N0}"));
        });
    }

    private void BuildKpiRow(IContainer container)
    {
        var totalWorkItems = GetTotalWorkItems();

        container.Row(row =>
        {
            row.Spacing(12);

            row.RelativeItem().Component(new StatCard("Teams", $"{_data.TeamsSummary.TotalTeamsCount:N0}", ReportPalette.Primary, ReportPalette.PrimarySoft));
            row.RelativeItem().Component(new StatCard("Projects", $"{_data.ProjectsSummary.TotalProjectsCount:N0}", ReportPalette.Accent, ReportPalette.AccentSoft));
            row.RelativeItem().Component(new StatCard("Sprints", $"{_data.SprintsSummary.TotalSprintsCount:N0}", ReportPalette.Indigo, ReportPalette.IndigoSoft));
            row.RelativeItem().Component(new StatCard("Users", $"{_data.UsersSummary.TotalUsersCount:N0}", ReportPalette.Warning, ReportPalette.WarningSoft));
            row.RelativeItem().Component(new StatCard("Work Items", $"{totalWorkItems:N0}", ReportPalette.Danger, ReportPalette.DangerSoft));
        });
    }

    private void BuildTenantProfileCard(IContainer container)
    {
        container.ApplyPageCard().Column(col =>
        {
            col.Spacing(10);

            col.Item().Text("Tenant Profile")
                .Style(ReportTextStyles.CardTitle);

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.ConstantColumn(220);
                });

                table.Header(header =>
                {
                    header.Cell().Background(ReportPalette.Soft).Padding(10).Text("Field").Style(ReportTextStyles.TableHeader);
                    header.Cell().Background(ReportPalette.Soft).Padding(10).AlignRight().Text("Value").Style(ReportTextStyles.TableHeader);
                });

                AddRow(table, "Tenant Name", _data.Tenant.Name, 1);
                AddRow(table, "Domain", _data.Tenant.Domain, 2);
                AddRow(table, "Status", _data.Tenant.IsActive ? "Active" : "Inactive", 3);
                AddRow(table, "Country", DisplayOrDash(_data.Tenant.Country), 4);
                AddRow(table, "Website", DisplayOrDash(_data.Tenant.WebsiteUrl), 5);
                AddRow(table, "Created At", _data.Tenant.CreatedAt.ToString("dd MMM yyyy"), 6);
                AddRow(table, "Profile Image URL", DisplayOrDash(_data.Tenant.ProfileImageUrl), 7);
            });

            col.Item().PaddingTop(8).Text("Description")
                .Style(ReportTextStyles.CardTitle);

            col.Item().Text(DisplayOrDash(_data.Tenant.Description))
                .Style(ReportTextStyles.Body);
        });
    }

    private void BuildOrganizationSummaryCard(IContainer container)
    {
        container.ApplyPageCard().Column(col =>
        {
            col.Spacing(10);

            col.Item().Text("Organization Summary")
                .Style(ReportTextStyles.CardTitle);

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.ConstantColumn(120);
                });

                table.Header(header =>
                {
                    header.Cell().Background(ReportPalette.Soft).Padding(10).Text("Metric").Style(ReportTextStyles.TableHeader);
                    header.Cell().Background(ReportPalette.Soft).Padding(10).AlignRight().Text("Value").Style(ReportTextStyles.TableHeader);
                });

                AddRow(table, "Total Teams", $"{_data.TeamsSummary.TotalTeamsCount:N0}", 0);
                AddRow(table, "Active Teams", $"{_data.TeamsSummary.ActiveTeamsCount:N0}", 1);
                AddRow(table, "Inactive Teams", $"{_data.TeamsSummary.InActiveTeamsCount:N0}", 2);
                AddRow(table, "Total Projects", $"{_data.ProjectsSummary.TotalProjectsCount:N0}", 3);
                AddRow(table, "Not Started Projects", $"{_data.ProjectsSummary.NotStartedProjectsCount:N0}", 4);
                AddRow(table, "In Progress Projects", $"{_data.ProjectsSummary.InProgressProjectsCount:N0}", 5);
                AddRow(table, "Completed Projects", $"{_data.ProjectsSummary.CompletedProjectsCount:N0}", 6);
                AddRow(table, "On Hold Projects", $"{_data.ProjectsSummary.OnHoldProjectsCount:N0}", 7);
                AddRow(table, "Cancelled Projects", $"{_data.ProjectsSummary.CancelledProjectsCount:N0}", 8);
                AddRow(table, "Total Users", $"{_data.UsersSummary.TotalUsersCount:N0}", 9);
            });
        });
    }

    private void BuildWorkItemsSummaryCard(IContainer container)
    {
        container.ApplyPageCard().Column(col =>
        {
            col.Spacing(10);

            col.Item().Text("Work Items Summary")
                .Style(ReportTextStyles.CardTitle);

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.ConstantColumn(120);
                });

                table.Header(header =>
                {
                    header.Cell().Background(ReportPalette.Soft).Padding(10).Text("Metric").Style(ReportTextStyles.TableHeader);
                    header.Cell().Background(ReportPalette.Soft).Padding(10).AlignRight().Text("Value").Style(ReportTextStyles.TableHeader);
                });

                AddRow(table, "Epics", $"{_data.WorkItemsSummary.EpicsCount:N0}", 0);
                AddRow(table, "User Stories", $"{_data.WorkItemsSummary.UserStoriesCount:N0}", 1);
                AddRow(table, "Tasks", $"{_data.WorkItemsSummary.TaskItemCount:N0}", 2);
                AddRow(table, "Bugs", $"{_data.WorkItemsSummary.BugsCount:N0}", 3);
                AddRow(table, "Test Cases", $"{_data.WorkItemsSummary.TestCasesCount:N0}", 4);
                AddRow(table, "Total Work Items", $"{GetTotalWorkItems():N0}", 5);
            });
        });
    }

    private void BuildProjectAndSprintHealthCard(IContainer container)
    {
        container.ApplyPageCard().Column(col =>
        {
            col.Spacing(10);

            col.Item().Text("Project & Sprint Health")
                .Style(ReportTextStyles.CardTitle);

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.ConstantColumn(120);
                });

                table.Header(header =>
                {
                    header.Cell().Background(ReportPalette.Soft).Padding(10).Text("Metric").Style(ReportTextStyles.TableHeader);
                    header.Cell().Background(ReportPalette.Soft).Padding(10).AlignRight().Text("Value").Style(ReportTextStyles.TableHeader);
                });

                AddRow(table, "Total Sprints", $"{_data.SprintsSummary.TotalSprintsCount:N0}", 0);
                AddRow(table, "Planned Sprints", $"{_data.SprintsSummary.PlannedSprintsCount:N0}", 1);
                AddRow(table, "Active Sprints", $"{_data.SprintsSummary.ActiveSprintsCount:N0}", 2);
                AddRow(table, "Completed Sprints", $"{_data.SprintsSummary.CompletedSprintsCount:N0}", 3);
                AddRow(table, "Cancelled Sprints", $"{_data.SprintsSummary.CancelledSprintsCount:N0}", 4);
                AddRow(table, "In Progress Projects", $"{_data.ProjectsSummary.InProgressProjectsCount:N0}", 5);
                AddRow(table, "Completed Projects", $"{_data.ProjectsSummary.CompletedProjectsCount:N0}", 6);
                AddRow(table, "On Hold Projects", $"{_data.ProjectsSummary.OnHoldProjectsCount:N0}", 7);
            });
        });
    }

    private int GetTotalWorkItems()
    {
        return _data.WorkItemsSummary.EpicsCount +
               _data.WorkItemsSummary.UserStoriesCount +
               _data.WorkItemsSummary.TaskItemCount +
               _data.WorkItemsSummary.BugsCount +
               _data.WorkItemsSummary.TestCasesCount;
    }

    private static string DisplayOrDash(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "-" : value.Trim();
    }

    private static void AddRow(TableDescriptor table, string label, string value, int index)
    {
        var bg = index % 2 == 0 ? ReportPalette.White : ReportPalette.Soft;

        table.Cell().Background(bg).Padding(10).Text(label).FontColor(ReportPalette.Ink);
        table.Cell().Background(bg).Padding(10).AlignRight().Text(value).SemiBold().FontColor(ReportPalette.Ink);
    }

    private void BuildFooter(IContainer container)
    {
        ReportLayoutHelpers.BuildStandardFooter(container, "Generated by Collabrium Platform");
    }
}