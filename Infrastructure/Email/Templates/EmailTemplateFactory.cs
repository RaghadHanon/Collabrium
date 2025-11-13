using Application.Interfaces.Emails;
using System.Reflection;

namespace Infrastructure.Email.Templates;

public class EmailTemplateFactory : IEmailTemplateFactory
{
    private readonly Dictionary<string, IEmailTemplate> _templates = new();

    public EmailTemplateFactory()
    {
        var templateTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(IEmailTemplate).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var type in templateTypes)
        {
            var instance = (IEmailTemplate) Activator.CreateInstance(type)!;
            _templates[type.Name.Replace("EmailTemplate", "")] = instance;
        }
    }

    public IEmailTemplate GetTemplate(string name)
    {
        return !_templates.ContainsKey(name)
            ? throw new InvalidOperationException($"No email template found for '{name}'.")
            : _templates[name];
    }
}
