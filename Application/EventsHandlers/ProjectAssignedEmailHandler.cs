using Application.Common;
using Application.Events;
using Application.Interfaces.Emails;
using MediatR;
using Microsoft.Extensions.Options;

namespace Application.EventsHandlers;

public class ProjectAssignedEmailHandler
    : INotificationHandler<ProjectAssignedEvent>
{
    private readonly IEmailService emailService;
    private readonly IEmailTemplateFactory templateFactory;
    private readonly WebAppSettings settings;

    public ProjectAssignedEmailHandler(
        IEmailService emailService,
        IEmailTemplateFactory templateFactory,
        IOptions<WebAppSettings> settings)
    {
        this.emailService = emailService;
        this.templateFactory = templateFactory;
        this.settings = settings.Value;
    }

    public async Task Handle(ProjectAssignedEvent notification, CancellationToken ct)
    {
        var template = templateFactory.GetTemplate("ProjectAssignment");

        var url = $"{settings.BaseUrl}/dashboard/projects/{notification.ProjectId}";

        for (var i = 0; i < notification.TeamMembers.Count; i++)
        {
            var member = notification.TeamMembers[i];

            var data = new Dictionary<string, string>
            {
                ["AssigneeFullName"] = member.FullName,
                ["AssignerFullName"] = notification.AssignerFullName,
                ["TeamName"] = notification.TeamName,
                ["ProjectName"] = notification.ProjectTitle,
                ["ProjectKey"] = notification.ProjectKey,
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