using Domain.Entities;

namespace Application.Interfaces.Emails;

public interface IEmailService
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
