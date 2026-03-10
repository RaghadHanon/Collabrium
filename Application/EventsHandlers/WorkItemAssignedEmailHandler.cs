using Application.Common;
using Application.Interfaces.Emails;
using Domain.Events;
using MediatR;
using Microsoft.Extensions.Options;

namespace Application.EventsHandlers;
public class WorkItemAssignedEmailHandler
    : INotificationHandler<WorkItemAssignedEvent>
{
    private readonly IEmailService emailService;
    private readonly IEmailTemplateFactory templateFactory;
    private readonly WebAppSettings settings;

    public WorkItemAssignedEmailHandler(
        IEmailService emailService,
        IEmailTemplateFactory templateFactory,
        IOptions<WebAppSettings> settings)
    {
        this.emailService = emailService;
        this.templateFactory = templateFactory;
        this.settings = settings.Value;
    }

    public async Task Handle(WorkItemAssignedEvent notification, CancellationToken ct)
    {
        var template = templateFactory.GetTemplate("WorkItemAssignment");

        var url = $"{settings.BaseUrl}/api/projects/{notification.ProjectId}/work-items/{notification.WorkItemId}";

        var data = new Dictionary<string, string>
        {
            ["AssigneeFullName"] = notification.AssigneeFullName,
            ["AssignerFullName"] = notification.AssignerFullName,
            ["WorkItemCode"] = notification.WorkItemCode,
            ["WorkItemTitle"] = notification.WorkItemTitle,
            ["WorkItemType"] = notification.WorkItemType,
            ["WorkItemPriority"] = notification.WorkItemPriority,
            ["Url"] = url
        };

        var message = new EmailMessageBuilder()
            .To(notification.AssigneeEmail)
            .Subject(template.GetSubject())
            .PlainTextBody(template.GetPlainTextBody(data))
            .HtmlBody(template.GetHtmlBody(data))
            .Build();

        await emailService.SendAsync(message, ct);
    }
}