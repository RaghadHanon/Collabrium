namespace Application.Interfaces.AI;

public interface IAiAssistantService
{
    Task<string> AskAsync(string prompt, CancellationToken cancellationToken = default);
}