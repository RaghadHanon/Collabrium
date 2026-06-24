using System.Net.Http.Json;
using System.Text.Json;
using Application.Interfaces.AI;

namespace Infrastructure.AI;

public sealed class OllamaAiAssistantService(HttpClient httpClient) : IAiAssistantService
{
    private const string DefaultModel = "gemma3:latest";

    public async Task<string> AskAsync(
        string prompt,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            throw new ArgumentException("Prompt is required.", nameof(prompt));

        var request = new
        {
            model = DefaultModel,
            stream = false,
            messages = new[]
            {
                new
                {
                    role = "system",
                    content = """
                    You are an AI assistant inside Collabrium.
                    Follow the user's instructions exactly.
                    Use only the information provided in the prompt.
                    Do not invent facts, IDs, names, numbers, or system data.
                    Return the response in the format requested by the prompt.
                    """
                },
                new
                {
                    role = "user",
                    content = prompt.Trim()
                }
            }
        };

        using var response = await httpClient.PostAsJsonAsync(
            "/api/chat",
            request,
            cancellationToken);

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Ollama request failed with status code {(int) response.StatusCode}. Response: {responseBody}");
        }

        return ExtractAssistantContent(responseBody);
    }

    private static string ExtractAssistantContent(string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
            throw new InvalidOperationException("Ollama returned an empty response.");

        try
        {
            using var json = JsonDocument.Parse(responseBody);

            if (!json.RootElement.TryGetProperty("message", out var messageElement))
                throw new InvalidOperationException("Ollama response does not contain a message object.");

            if (!messageElement.TryGetProperty("content", out var contentElement))
                throw new InvalidOperationException("Ollama response does not contain message content.");

            return contentElement.GetString() ?? string.Empty;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Ollama returned invalid JSON. Details: {ex.Message}");
        }
    }
}