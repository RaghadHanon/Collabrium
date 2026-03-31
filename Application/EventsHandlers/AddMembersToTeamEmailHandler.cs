using Application.Common;
using Application.Events;
using Application.Interfaces.Emails;
using MediatR;
using Microsoft.Extensions.Options;

namespace Application.EventsHandlers;

public class AddMembersToTeamEmailHandler
    : INotificationHandler<AddMemberToTeamEvent>
{
    private readonly IEmailService emailService;
    private readonly IEmailTemplateFactory templateFactory;
    private readonly WebAppSettings settings;

    public AddMembersToTeamEmailHandler(
        IEmailService emailService,
        IEmailTemplateFactory templateFactory,
        IOptions<WebAppSettings> settings)
    {
        this.emailService = emailService;
        this.templateFactory = templateFactory;
        this.settings = settings.Value;
    }

    public async Task Handle(AddMemberToTeamEvent notification, CancellationToken ct)
    {
        var template = templateFactory.GetTemplate("AddMemberToTeam");

        var url = $"{settings.BaseUrl}/dashboard/teams/{notification.TeamId}";

        for (int i = 0; i < notification.TeamMembers.Count; i++)
        {
            var member = notification.TeamMembers[i];
            var data = new Dictionary<string, string>
            {
                ["AssigneeFullName"] = member.FullName,
                ["AssignerFullName"] = notification.AssignerFullName,
                ["TeamLeadName"] = notification.TeamLeadName,
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