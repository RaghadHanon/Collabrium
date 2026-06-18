using Application.Features.AI.DTOs.Roadmaps;
using Application.Interfaces.AI;
using Domain.Enums;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Infrastructure.AI;
public sealed class RoadmapAiDraftService(
    IAiAssistantService aiAssistantService)
    : IRoadmapAiDraftService
{
    private const int MaxAttempts = 3;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    public async Task<RoadmapFullDraftDto> GenerateFullDraftAsync(
        RoadmapPath path,
        RoadmapLevel level,
        string goal,
        int topicsCount,
        int subTopicsPerTopic,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(goal))
            throw new ArgumentException("Goal is required.", nameof(goal));

        goal = goal.Trim();

        var prompt = BuildPrompt(path, level, goal, topicsCount, subTopicsPerTopic);

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            var aiResponse = await aiAssistantService.AskAsync(prompt, cancellationToken);

            if (TryParse(aiResponse, path, level, goal, topicsCount, subTopicsPerTopic, out var draft))
                return draft!;

            prompt = BuildRepairPrompt(path, level, goal, topicsCount, subTopicsPerTopic, aiResponse);
        }

        return BuildFallback(path, level, goal);
    }

    private static string BuildPrompt(
        RoadmapPath path,
        RoadmapLevel level,
        string goal,
        int topicsCount,
        int subTopicsPerTopic)
    {
        return $$"""
        You are an AI learning roadmap assistant inside Collabrium.

        Generate a complete learning roadmap draft.

        User goal:
        {{goal}}

        Roadmap path: {{path}}
        Roadmap level: {{level}}

        Required counts:
        - topics count: {{topicsCount}}
        - subtopics per topic: {{subTopicsPerTopic}}

        Important:
        - Generate Topic nodes only.
        - Do not generate Reference nodes.
        - Do not generate resources.
        - Do not include URLs.
        - Do not include resource objects.
        - Do not include IDs.
        - Do not include tenantId, userId, createdAt, updatedAt, or database fields.

        Rules:
        - Return valid JSON only.
        - Do not wrap JSON in markdown.
        - Do not add explanations.
        - Every topic must have an order starting from 1.
        - Every subtopic must have an order starting from 1 inside its topic.
        - Order the roadmap from basics to advanced.
        - Keep titles clear and practical.
        - Keep descriptions concise.
        - Make the content suitable for the selected path and level.
        - Suggested tags should be short technology or skill names.

        Required JSON schema:
        {
          "title": "string",
          "description": "string",
          "suggestedTags": ["string"],
          "topics": [
            {
              "order": 1,
              "title": "string",
              "description": "string",
              "subTopics": [
                {
                  "order": 1,
                  "title": "string",
                  "description": "string"
                }
              ]
            }
          ]
        }
        """;
    }

    private static string BuildRepairPrompt(
        RoadmapPath path,
        RoadmapLevel level,
        string goal,
        int topicsCount,
        int subTopicsPerTopic,
        string invalidResponse)
    {
        return $$"""
        Your previous response was invalid.

        Fix it and return valid JSON only.

        User goal:
        {{goal}}

        Roadmap path: {{path}}
        Roadmap level: {{level}}

        Required counts:
        - topics count: {{topicsCount}}
        - subtopics per topic: {{subTopicsPerTopic}}

        Previous response:
        {{invalidResponse}}

        Rules:
        - Return JSON only.
        - No markdown.
        - No explanations.
        - No IDs.
        - No URLs.
        - No resources.
        - Do not generate Reference nodes.
        - Generate Topic nodes only.
        - Include order for each topic.
        - Include order for each subtopic.

        Required JSON schema:
        {
          "title": "string",
          "description": "string",
          "suggestedTags": ["string"],
          "topics": [
            {
              "order": 1,
              "title": "string",
              "description": "string",
              "subTopics": [
                {
                  "order": 1,
                  "title": "string",
                  "description": "string"
                }
              ]
            }
          ]
        }
        """;
    }

    private static bool TryParse(
        string response,
        RoadmapPath path,
        RoadmapLevel level,
        string goal,
        int topicsCount,
        int subTopicsPerTopic,
        out RoadmapFullDraftDto? draft)
    {
        draft = null;

        try
        {
            var json = ExtractJson(response);

            draft = JsonSerializer.Deserialize<RoadmapFullDraftDto>(json, JsonOptions);

            if (draft is null)
                return false;

            Normalize(draft, path, level, goal, topicsCount, subTopicsPerTopic);

            return !string.IsNullOrWhiteSpace(draft.Title)
                   && draft.Topics.Count > 0
                   && draft.Topics.All(t => !string.IsNullOrWhiteSpace(t.Title));
        }
        catch
        {
            return false;
        }
    }

    private static void Normalize(
        RoadmapFullDraftDto draft,
        RoadmapPath path,
        RoadmapLevel level,
        string goal,
        int topicsCount,
        int subTopicsPerTopic)
    {
        draft.Title = NormalizeText(draft.Title, $"{level} {path} Roadmap");
        draft.Description = NormalizeText(draft.Description, goal);

        draft.SuggestedTags ??= new();
        draft.SuggestedTags = draft.SuggestedTags
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(10)
            .ToList();

        draft.Topics ??= new();

        draft.Topics = draft.Topics
            .Where(x => !string.IsNullOrWhiteSpace(x.Title))
            .Take(topicsCount)
            .ToList();

        for (var i = 0; i < draft.Topics.Count; i++)
        {
            var topic = draft.Topics[i];

            topic.Order = i + 1;
            topic.Title = NormalizeText(topic.Title, $"Topic {i + 1}");
            topic.Description = NormalizeText(topic.Description, $"Learn an important part of {path}.");

            topic.SubTopics ??= new();

            topic.SubTopics = topic.SubTopics
                .Where(x => !string.IsNullOrWhiteSpace(x.Title))
                .Take(subTopicsPerTopic)
                .ToList();

            for (var j = 0; j < topic.SubTopics.Count; j++)
            {
                var subTopic = topic.SubTopics[j];

                subTopic.Order = j + 1;
                subTopic.Title = NormalizeText(subTopic.Title, $"Subtopic {j + 1}");
                subTopic.Description = NormalizeText(
                    subTopic.Description,
                    "Learn the main concepts and practical usage.");
            }
        }
    }

    private static RoadmapFullDraftDto BuildFallback(
        RoadmapPath path,
        RoadmapLevel level,
        string goal)
    {
        return new RoadmapFullDraftDto
        {
            Title = $"{level} {path} Roadmap",
            Description = goal,
            SuggestedTags = new()
            {
                path.ToString(),
                level.ToString()
            },
            Topics = new()
            {
                new()
                {
                    Order = 1,
                    Title = "Fundamentals",
                    Description = $"Learn the core fundamentals of {path}.",
                    SubTopics = new()
                    {
                        new()
                        {
                            Order = 1,
                            Title = "Core Concepts",
                            Description = "Understand the basic concepts before moving forward."
                        }
                    }
                }
            }
        };
    }

    private static string ExtractJson(string response)
    {
        if (string.IsNullOrWhiteSpace(response))
            throw new InvalidOperationException("AI returned empty response.");

        var trimmed = response.Trim();

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
            throw new InvalidOperationException("No JSON object found.");

        return jsonMatch.Value.Trim();
    }

    private static string NormalizeText(string? value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value)
            ? fallback.Trim()
            : value.Trim();
    }
}