using System.Net.Http.Json;
using System.Text.Json;
using Application.Interfaces.AI;

namespace Infrastructure.AI;

public sealed class OllamaAiAssistantService(HttpClient httpClient) : IAiAssistantService
{
    public async Task<string> AskAsync(
        string prompt,
        CancellationToken cancellationToken = default)
    {
        var request = new
        {
            model = "gemma3",
            stream = false,
            messages = new[]
            {
                new
                {
                    role = "system",
                    content = """
                    You are an AI assistant inside Collabrium.
                    Analyze reports and give clear, professional, useful insights.
                    Do not invent data.
                    Use only the provided input.
                    """
                },
                new
                {
                    role = "user",
                    content = prompt
                }
            }
        };

        var response = await httpClient.PostAsJsonAsync(
            "/api/chat",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        using var json = await JsonDocument.ParseAsync(
            stream,
            cancellationToken: cancellationToken);

        return json.RootElement
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? string.Empty;
    }
}