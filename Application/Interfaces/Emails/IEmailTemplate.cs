namespace Application.Interfaces.Emails;

public interface IEmailTemplate
{
    string GetSubject();
    string GetPlainTextBody(Dictionary<string, string> data);
    string GetHtmlBody(Dictionary<string, string> data);
}
