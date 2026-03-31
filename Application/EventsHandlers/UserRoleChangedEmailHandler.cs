using Application.Common;
using Application.Events;
using Application.Interfaces.Emails;
using MediatR;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace Application.EventsHandlers;

public class UserRoleChangedEmailHandler
    : INotificationHandler<UserRoleChangedEvent>
{
    private readonly IEmailService emailService;
    private readonly IEmailTemplateFactory templateFactory;

    public UserRoleChangedEmailHandler(
        IEmailService emailService,
        IEmailTemplateFactory templateFactory)
    {
        this.emailService = emailService;
        this.templateFactory = templateFactory;
    }

    public async Task Handle(UserRoleChangedEvent notification, CancellationToken ct)
    {
        var template = templateFactory.GetTemplate("UserRoleChanged");
        

        var data = new Dictionary<string, string>
        {
            ["AssigneeFullName"] = notification.User.FullName,
            ["OldJobTitle"] = notification.OldJobTitle.Length > 3 ? Regex.Replace(notification.OldJobTitle.ToString(), "(\\B[A-Z])", " $1") : notification.OldJobTitle,
            ["NewJobTitle"] = notification.User.JobTitle.Length > 3 ? Regex.Replace(notification.User.JobTitle.ToString(), "(\\B[A-Z])", " $1") : notification.User.JobTitle,
            ["AssignerFullName"] = notification.AssignerFullName    
        };

        var message = new EmailMessageBuilder()
            .To(notification.User.Email)
            .Subject(template.GetSubject())
            .PlainTextBody(template.GetPlainTextBody(data))
            .HtmlBody(template.GetHtmlBody(data))
            .Build();

        await emailService.SendAsync(message, ct);
    }
}