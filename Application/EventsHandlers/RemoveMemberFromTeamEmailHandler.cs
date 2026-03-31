using Application.Common;
using Application.Events;
using Application.Interfaces.Emails;
using MediatR;

namespace Application.EventsHandlers;

public class RemoveMemberFromTeamEmailHandler
    : INotificationHandler<RemoveMemberFromTeamEvent>
{
    private readonly IEmailService emailService;
    private readonly IEmailTemplateFactory templateFactory;

    public RemoveMemberFromTeamEmailHandler(
        IEmailService emailService,
        IEmailTemplateFactory templateFactory)
    {
        this.emailService = emailService;
        this.templateFactory = templateFactory;
    }

    public async Task Handle(RemoveMemberFromTeamEvent notification, CancellationToken ct)
    {
        var template = templateFactory.GetTemplate("RemoveMemberFromTeam");

        for (var i = 0; i < notification.TeamMembers.Count; i++)
        {
            var member = notification.TeamMembers[i];
            var data = new Dictionary<string, string>
            {
                ["AssigneeFullName"] = member.FullName,
                ["AssignerFullName"] = notification.AssignerFullName,
                ["TeamLeadName"] = notification.TeamLeadName,
                ["TeamName"] = notification.TeamName
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