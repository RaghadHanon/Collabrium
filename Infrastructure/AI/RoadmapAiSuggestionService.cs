using Application.Features.AI.DTOs.Roadmaps;
using Application.Interfaces.AI;
using Domain.Enums;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Infrastructure.AI;
public sealed class RoadmapAiSuggestionService(
    IAiAssistantService aiAssistantService)
    : IRoadmapAiSuggestionService
{
    private const int MaxAttempts = 2;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    public Task<RoadmapTopicSuggestionsDto> SuggestTopicsAsync(
        string roadmapTitle,
        string? roadmapDescription,
        string path,
        string level,
        IReadOnlyCollection<string> existingTopicTitles,
        int suggestionsCount,
        CancellationToken cancellationToken = default)
    {
        var prompt = $$"""
        You are an AI learning roadmap assistant inside Collabrium.

        Suggest new topic nodes for an existing roadmap.

        Roadmap:
        - Title: {{roadmapTitle}}
        - Description: {{roadmapDescription ?? "N/A"}}
        - Path: {{path}}
        - Level: {{level}}

        Existing topics:
        {{FormatList(existingTopicTitles)}}

        Rules:
        - Return valid JSON only.
        - Do not use markdown.
        - Suggest exactly {{suggestionsCount}} new topics.
        - Do not repeat or slightly rename existing topics.
        - Do not generate subtopics.
        - Do not generate resources.
        - Keep titles short and practical.
        - Keep descriptions concise.
         Description rules:
        - Each description should be 1-2 sentences.
        - Around 15-27 words.
        - Explain the purpose of the topic and what skills the learner will acquire.

        Required JSON:
        {
          "suggestions": [
            {
              "title": "string",
              "description": "string"
            }
          ]
        }
        """;

        return GenerateAsync<RoadmapTopicSuggestionsDto>(
            prompt,
            NormalizeTopicSuggestions,
            BuildFallbackTopicSuggestions(roadmapTitle, suggestionsCount),
            cancellationToken);
    }

    public Task<RoadmapSubTopicSuggestionsDto> SuggestSubTopicsAsync(
        string roadmapTitle,
        string topicTitle,
        string? topicDescription,
        IReadOnlyCollection<string> existingSubTopicTitles,
        int suggestionsCount,
        CancellationToken cancellationToken = default)
    {
        var prompt = $$"""
        You are an AI learning roadmap assistant inside Collabrium.

        Suggest new subtopics for an existing topic.

        Roadmap title:
        {{roadmapTitle}}

        Topic:
        - Title: {{topicTitle}}
        - Description: {{topicDescription ?? "N/A"}}

        Existing subtopics:
        {{FormatList(existingSubTopicTitles)}}

        Rules:
        - Return valid JSON only.
        - Do not use markdown.
        - Suggest exactly {{suggestionsCount}} new subtopics.
        - Do not repeat or slightly rename existing subtopics.
        - Do not generate resources.
        - Keep titles short and practical.
        - Keep descriptions concise.
         Description rules:
        - Each description should be 1-2 sentences.
        - Around 15-27 words.
        - Explain the purpose of the topic and what skills the learner will acquire.
        Required JSON:
        {
          "suggestions": [
            {
              "title": "string",
              "description": "string"
            }
          ]
        }
        """;

        return GenerateAsync<RoadmapSubTopicSuggestionsDto>(
            prompt,
            NormalizeSubTopicSuggestions,
            BuildFallbackSubTopicSuggestions(topicTitle, suggestionsCount),
            cancellationToken);
    }

    public Task<RoadmapResourceSuggestionsDto> SuggestResourcesAsync(
        string roadmapTitle,
        string topicTitle,
        string subTopicTitle,
        string? subTopicDescription,
        IReadOnlyCollection<string> existingResourceTitles,
        int suggestionsCount,
        CancellationToken cancellationToken = default)
    {
        var resourceTypes = string.Join(", ", Enum.GetNames<RoadmapResourceType>());

        var prompt = $$"""
        You are an AI learning roadmap assistant inside Collabrium.

        Suggest learning resources for an existing subtopic.

        Roadmap title:
        {{roadmapTitle}}

        Topic title:
        {{topicTitle}}

        Subtopic:
        - Title: {{subTopicTitle}}
        - Description: {{subTopicDescription ?? "N/A"}}

        Existing resources:
        {{FormatList(existingResourceTitles)}}

        Rules:
        - Return valid JSON only.
        - Do not use markdown.
        - Suggest exactly {{suggestionsCount}} resources.
        - Do not repeat existing resources.
        - Do not invent URLs.
        - Use searchKeyword instead of url.
        - Resource type must be exactly one of: {{resourceTypes}}.
        - Prefer official documentation, practical articles, videos, or tools.
        - Keep titles clear.
                Description rules:
        - Each description should be 1-2 sentences.
        - Around 15-27 words.
        - Explain the purpose of the topic and what skills the learner will acquire.
        Required JSON:
        {
          "suggestions": [
            {
              "title": "string",
              "type": "Documentation",
              "searchKeyword": "string"
            }
          ]
        }
        """;

        return GenerateAsync<RoadmapResourceSuggestionsDto>(
            prompt,
            NormalizeResourceSuggestions,
            BuildFallbackResourceSuggestions(subTopicTitle, suggestionsCount),
            cancellationToken);
    }

    private async Task<T> GenerateAsync<T>(
        string initialPrompt,
        Action<T> normalize,
        T fallback,
        CancellationToken cancellationToken)
    {
        var prompt = initialPrompt;

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            var aiResponse = await aiAssistantService.AskAsync(prompt, cancellationToken);

            if (TryParse(aiResponse, normalize, out T? result) && result is not null)
                return result;

            prompt = $$"""
            Your previous response was invalid.

            Fix it and return valid JSON only.
            Do not use markdown.
            Do not add explanations.

            Previous response:
            {{aiResponse}}
            """;
        }

        return fallback;
    }

    private static bool TryParse<T>(
        string aiResponse,
        Action<T> normalize,
        out T? result)
    {
        result = default;

        try
        {
            var json = ExtractJson(aiResponse);

            result = JsonSerializer.Deserialize<T>(
                json,
                JsonOptions);

            if (result is null)
                return false;

            normalize(result);

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string ExtractJson(string aiResponse)
    {
        if (string.IsNullOrWhiteSpace(aiResponse))
            throw new InvalidOperationException("AI returned an empty response.");

        var trimmed = aiResponse.Trim();

        if (trimmed.StartsWith("{") && trimmed.EndsWith("}"))
            return trimmed;

        var codeBlockMatch = Regex.Match(
            trimmed,
            @"```(?:json)?\s*(\{[\s\S]*?\})\s*```",
            RegexOptions.IgnoreCase | RegexOptions.Multiline);

        if (codeBlockMatch.Success)
            return codeBlockMatch.Groups[1].Value.Trim();

        var jsonMatch = Regex.Match(
            trimmed,
            @"\{[\s\S]*\}",
            RegexOptions.Multiline);

        if (!jsonMatch.Success)
            throw new InvalidOperationException("AI response does not contain JSON.");

        return jsonMatch.Value.Trim();
    }

    private static string FormatList(IReadOnlyCollection<string> items)
    {
        if (items.Count == 0)
            return "- None";

        return string.Join(
            Environment.NewLine,
            items.Select(x => $"- {x}"));
    }

    private static void NormalizeTopicSuggestions(RoadmapTopicSuggestionsDto dto)
    {
        dto.Suggestions ??= new();

        dto.Suggestions = dto.Suggestions
            .Where(x => !string.IsNullOrWhiteSpace(x.Title))
            .Select(x =>
            {
                x.Title = x.Title.Trim();
                x.Description = NormalizeText(x.Description, "Suggested roadmap topic.");
                return x;
            })
            .DistinctBy(x => x.Title.ToLowerInvariant())
            .Take(5)
            .ToList();
    }

    private static void NormalizeSubTopicSuggestions(RoadmapSubTopicSuggestionsDto dto)
    {
        dto.Suggestions ??= new();

        dto.Suggestions = dto.Suggestions
            .Where(x => !string.IsNullOrWhiteSpace(x.Title))
            .Select(x =>
            {
                x.Title = x.Title.Trim();
                x.Description = NormalizeText(x.Description, "Suggested topic subtopic.");
                return x;
            })
            .DistinctBy(x => x.Title.ToLowerInvariant())
            .Take(5)
            .ToList();
    }

    private static void NormalizeResourceSuggestions(RoadmapResourceSuggestionsDto dto)
    {
        dto.Suggestions ??= new();

        dto.Suggestions = dto.Suggestions
            .Where(x => !string.IsNullOrWhiteSpace(x.Title))
            .Select(x =>
            {
                x.Title = x.Title.Trim();
                x.SearchKeyword = NormalizeText(x.SearchKeyword, x.Title);

                if (!Enum.IsDefined(typeof(RoadmapResourceType), x.Type))
                    x.Type = RoadmapResourceType.Documentation;

                return x;
            })
            .DistinctBy(x => x.Title.ToLowerInvariant())
            .Take(5)
            .ToList();
    }

    private static RoadmapTopicSuggestionsDto BuildFallbackTopicSuggestions(
        string roadmapTitle,
        int count)
    {
        return new RoadmapTopicSuggestionsDto
        {
            Suggestions = Enumerable.Range(1, count)
                .Select(i => new RoadmapTopicSuggestionDto
                {
                    Title = $"Suggested Topic {i}",
                    Description = $"A suggested topic for {roadmapTitle}."
                })
                .ToList()
        };
    }

    private static RoadmapSubTopicSuggestionsDto BuildFallbackSubTopicSuggestions(
        string topicTitle,
        int count)
    {
        return new RoadmapSubTopicSuggestionsDto
        {
            Suggestions = Enumerable.Range(1, count)
                .Select(i => new RoadmapSubTopicSuggestionDto
                {
                    Title = $"Suggested Subtopic {i}",
                    Description = $"A suggested subtopic for {topicTitle}."
                })
                .ToList()
        };
    }

    private static RoadmapResourceSuggestionsDto BuildFallbackResourceSuggestions(
        string subTopicTitle,
        int count)
    {
        return new RoadmapResourceSuggestionsDto
        {
            Suggestions = Enumerable.Range(1, count)
                .Select(i => new RoadmapResourceSuggestionDto
                {
                    Title = $"Suggested Resource {i}",
                    Type = RoadmapResourceType.Documentation,
                    SearchKeyword = $"{subTopicTitle} learning resource"
                })
                .ToList()
        };
    }

    private static string NormalizeText(string? value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value)
            ? fallback
            : value.Trim();
    }
}