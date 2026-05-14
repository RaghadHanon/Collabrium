using Application.Common;
using Application.Events;
using Application.Interfaces.Emails;
using MediatR;
using Microsoft.Extensions.Options;

namespace Application.EventsHandlers;

public class ToggleTeamActivationEmailHandler
    : INotificationHandler<ToggleTeamActivationEvent>
{
    private readonly IEmailService emailService;
    private readonly IEmailTemplateFactory templateFactory;
    private readonly WebAppSettings settings;

    public ToggleTeamActivationEmailHandler(
        IEmailService emailService,
        IEmailTemplateFactory templateFactory,
        IOptions<WebAppSettings> settings)
    {
        this.emailService = emailService;
        this.templateFactory = templateFactory;
        this.settings = settings.Value;
    }

    public async Task Handle(ToggleTeamActivationEvent notification, CancellationToken ct)
    {
        var template = templateFactory.GetTemplate("ToggleTeamActivation");

        var url = $"{settings.BaseUrl}/dashboard/teams/{notification.TeamId}";

        for (var i = 0; i < notification.TeamMembers.Count; i++)
        {
            var member = notification.TeamMembers[i];
            var data = new Dictionary<string, string>
            {
                ["AssigneeFullName"] = member.FullName,
                ["AssignerFullName"] = notification.AssignerFullName,
                ["IsActivated"] = notification.IsActivated ? "activated" : "deactivated",
                ["TeamName"] = notification.TeamName,
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