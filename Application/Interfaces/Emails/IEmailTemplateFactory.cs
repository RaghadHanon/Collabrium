namespace Application.Interfaces.Emails;

public interface IEmailTemplateFactory
{
    IEmailTemplate GetTemplate(string name);
}