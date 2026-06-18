using Application.Common;
using Application.Events;
using Application.Interfaces.Emails;
using MediatR;
using Microsoft.Extensions.Options;

namespace Application.EventsHandlers;

public class CourseAssignedEmailHandler
    : INotificationHandler<CourseAssignedEvent>
{
    private readonly IEmailService emailService;
    private readonly IEmailTemplateFactory templateFactory;
    private readonly WebAppSettings settings;

    public CourseAssignedEmailHandler(
        IEmailService emailService,
        IEmailTemplateFactory templateFactory,
        IOptions<WebAppSettings> settings)
    {
        this.emailService = emailService;
        this.templateFactory = templateFactory;
        this.settings = settings.Value;
    }

    public async Task Handle(CourseAssignedEvent notification, CancellationToken ct)
    {
        var template = templateFactory.GetTemplate("CourseAssignment");

        var url = $"{settings.BaseUrl}/learning-hub/courses/{notification.CourseId}";

        var data = new Dictionary<string, string>
        {
            ["AssigneeFullName"] = notification.AssignedTo.FullName,
            ["AssignerFullName"] = notification.AssignerFullName,
            ["CourseTitle"] = notification.CourseTitle,
            ["CourseProvider"] = notification.CourseProvider,
            ["Url"] = url
        };

        var message = new EmailMessageBuilder()
            .To(notification.AssignedTo.Email)
            .Subject(template.GetSubject())
            .PlainTextBody(template.GetPlainTextBody(data))
            .HtmlBody(template.GetHtmlBody(data))
            .Build();

        await emailService.SendAsync(message, ct);
    }
}