using Application.Interfaces.Emails;
using Domain.Entities;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;
using SendGrid.Helpers.Mail;
using System.Net;

namespace Infrastructure.Email;
public class SendGridEmailService : IEmailService
{
    private readonly SendGrid.SendGridClient _client;
    private readonly SendGridSettings _settings;

    public SendGridEmailService(IOptions<SendGridSettings> options)
    {
        _settings = options.Value;
        _client = new SendGrid.SendGridClient(_settings.ApiKey);
    }

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        var msg = new SendGridMessage();

        msg.SetFrom(new EmailAddress(_settings.From));
        msg.AddTo(message.To);
        msg.SetSubject(message.Subject);

        if (!string.IsNullOrEmpty(message.PlainTextBody))
            msg.AddContent("text/plain", message.PlainTextBody);

        if (!string.IsNullOrEmpty(message.HtmlBody))
            msg.AddContent("text/html", message.HtmlBody);

        foreach (var att in message.Attachments)
        {
            msg.AddAttachment(att.filename,
                Convert.ToBase64String(att.content),
                att.contentType);
        }

        var response = await _client.SendEmailAsync(msg, cancellationToken);
        if (response.StatusCode >= HttpStatusCode.BadRequest)
        {
            var body = await response.Body.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to send email via SendGrid: {(int) response.StatusCode} {body}");
        }
    }
}
