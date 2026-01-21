using Application.Interfaces.Emails;
using Domain.Entities;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace Infrastructure.Email;

public class SmtpEmailService : IEmailService
{
    private readonly SmtpSettings _settings;

    public SmtpEmailService(IOptions<SmtpSettings> options)
    {
        _settings = options.Value;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        var mail = new MailMessage();
        mail.From = new MailAddress(_settings.From);
        mail.To.Add(message.To);
        mail.Subject = message.Subject;
        mail.Body = message.HtmlBody ?? message.PlainTextBody;
        mail.IsBodyHtml = !string.IsNullOrEmpty(message.HtmlBody);

        foreach (var att in message.Attachments)
        {
            var stream = new MemoryStream(att.content);
            mail.Attachments.Add(new Attachment(stream, att.filename, att.contentType));
        }

        using var smtp = new SmtpClient(_settings.Host, _settings.Port)
        {
            Credentials = new NetworkCredential(_settings.User, _settings.Password),
            EnableSsl = true
        };

        await smtp.SendMailAsync(mail, cancellationToken);
    }
}
