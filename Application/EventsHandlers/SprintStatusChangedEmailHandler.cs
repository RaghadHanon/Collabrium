using Application.Common;
using Application.Events;
using Application.Interfaces.Emails;
using MediatR;
using Microsoft.Extensions.Options;

namespace Application.EventsHandlers;

public class SprintStatusChangedEmailHandler
    : INotificationHandler<SprintStatusChangedEvent>
{
    private readonly IEmailService emailService;
    private readonly IEmailTemplateFactory templateFactory;
    private readonly WebAppSettings settings;

    public SprintStatusChangedEmailHandler(
        IEmailService emailService,
        IEmailTemplateFactory templateFactory,
        IOptions<WebAppSettings> settings)
    {
        this.emailService = emailService;
        this.templateFactory = templateFactory;
        this.settings = settings.Value;
    }

    public async Task Handle(SprintStatusChangedEvent notification, CancellationToken ct)
    {
        var template = templateFactory.GetTemplate("SprintStatusChanged");

        var url = $"{settings.BaseUrl}/dashboard/projects/{notification.ProjectId}?tab=board&sprintId={notification.SprintId}";

        for (int i = 0; i < notification.TeamMembers.Count; i++)
        {
            var member = notification.TeamMembers[i];
            var data = new Dictionary<string, string>
            {
                ["AssigneeFullName"] = member.FullName,
                ["AssignerFullName"] = notification.AssignerFullName,
                ["ProjectName"] = notification.ProjectTitle,
                ["ProjectKey"] = notification.ProjectKey,
                ["SprintCode"] = notification.SprintCode,
                ["SprintName"] = notification.SprintName,
                ["SprintStatus"] = notification.Status.ToString(),
                ["Url"] = url
            };

            var message = new EmailMessageBuilder()
                .To(member.Email)
                .Subject(template.GetSubject())
                .PlainTextBody(template.GetPlainTextBody(data))
                .HtmlBody(template.GetHtmlBody(data))
                .Build();
            await emailService.SendAsync(message, ct);
        }
    }
}