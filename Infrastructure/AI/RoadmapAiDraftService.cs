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
        int resourcesPerSubTopic,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(goal))
            throw new ArgumentException("Goal is required.", nameof(goal));

        goal = goal.Trim();

        var prompt = BuildPrompt(
            path,
            level,
            goal,
            topicsCount,
            subTopicsPerTopic,
            resourcesPerSubTopic);

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            var aiResponse = await aiAssistantService.AskAsync(
                prompt,
                cancellationToken);

            if (TryParse(aiResponse, path, level, goal, out var draft))
                return draft!;

            prompt = BuildRepairPrompt(
                path,
                level,
                goal,
                topicsCount,
                subTopicsPerTopic,
                resourcesPerSubTopic,
                aiResponse);
        }

        return BuildFallback(path, level, goal);
    }

    private static string BuildPrompt(
        RoadmapPath path,
        RoadmapLevel level,
        string goal,
        int topicsCount,
        int subTopicsPerTopic,
        int resourcesPerSubTopic)
    {
        var resourceTypes = string.Join(", ", Enum.GetNames<RoadmapResourceType>());

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
        - resources per subtopic: {{resourcesPerSubTopic}}

        Important:
        - Generate Topic nodes only.
        - Do not generate Reference nodes.
        - Do not reference other roadmaps.
        - Do not include refRoadmapId.

        Rules:
        - Return valid JSON only.
        - Do not wrap JSON in markdown.
        - Do not add explanations.
        - Do not include IDs.
        - Do not include tenantId, userId, createdAt, updatedAt, or database fields.
        - Do not invent URLs.
        - For resources, use searchKeyword instead of url.
        - Resource type must be exactly one of: {{resourceTypes}}.
        - Keep titles clear and practical.
        - Keep descriptions concise.
        - Order the roadmap from basics to advanced.
        - Make the content suitable for the selected path and level.
        - Suggested tags should be short technology or skill names.
        - If resources count is 0, return empty resources arrays.

        Required JSON schema:
        {
          "title": "string",
          "description": "string",
          "suggestedTags": ["string"],
          "topics": [
            {
              "title": "string",
              "description": "string",
              "subTopics": [
                {
                  "title": "string",
                  "description": "string",
                  "resources": [
                    {
                      "title": "string",
                      "type": "Documentation",
                      "searchKeyword": "string"
                    }
                  ]
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
        int resourcesPerSubTopic,
        string invalidResponse)
    {
        var resourceTypes = string.Join(", ", Enum.GetNames<RoadmapResourceType>());

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
        - resources per subtopic: {{resourcesPerSubTopic}}

        Previous response:
        {{invalidResponse}}

        Rules:
        - Return JSON only.
        - No markdown.
        - No explanations.
        - No IDs.
        - No URLs.
        - Do not generate Reference nodes.
        - Generate Topic nodes only.
        - Use searchKeyword for resources.
        - Resource type must be one of: {{resourceTypes}}.

        Required JSON schema:
        {
          "title": "string",
          "description": "string",
          "suggestedTags": ["string"],
          "topics": [
            {
              "title": "string",
              "description": "string",
              "subTopics": [
                {
                  "title": "string",
                  "description": "string",
                  "resources": [
                    {
                      "title": "string",
                      "type": "Documentation",
                      "searchKeyword": "string"
                    }
                  ]
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
        out RoadmapFullDraftDto? draft)
    {
        draft = null;

        try
        {
            var json = ExtractJson(response);

            draft = JsonSerializer.Deserialize<RoadmapFullDraftDto>(
                json,
                JsonOptions);

            if (draft is null)
                return false;

            Normalize(draft, path, level, goal);

            return !string.IsNullOrWhiteSpace(draft.Title)
                   && draft.Topics.Count > 0
                   && draft.Topics.All(t => !string.IsNullOrWhiteSpace(t.Title));
        }
        catch
        {
            return false;
        }
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

    private static void Normalize(
        RoadmapFullDraftDto draft,
        RoadmapPath path,
        RoadmapLevel level,
        string goal)
    {
        draft.Title = NormalizeText(
            draft.Title,
            $"{level} {path} Roadmap");

        draft.Description = NormalizeText(
            draft.Description,
            goal);

        draft.SuggestedTags ??= new();

        draft.SuggestedTags = draft.SuggestedTags
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(10)
            .ToList();

        draft.Topics ??= new();

        foreach (var topic in draft.Topics)
        {
            topic.Title = NormalizeText(topic.Title, "Suggested Topic");

            topic.Description = NormalizeText(
                topic.Description,
                $"Learn an important part of {path}.");

            topic.SubTopics ??= new();

            foreach (var subTopic in topic.SubTopics)
            {
                subTopic.Title = NormalizeText(
                    subTopic.Title,
                    "Suggested Subtopic");

                subTopic.Description = NormalizeText(
                    subTopic.Description,
                    "Learn the main concepts and practical usage.");

                subTopic.Resources ??= new();

                foreach (var resource in subTopic.Resources)
                {
                    resource.Title = NormalizeText(
                        resource.Title,
                        subTopic.Title);

                    resource.SearchKeyword = NormalizeText(
                        resource.SearchKeyword,
                        $"{subTopic.Title} {path} learning resource");

                    if (!Enum.IsDefined(typeof(RoadmapResourceType), resource.Type))
                        resource.Type = RoadmapResourceType.Documentation;
                }
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
            SuggestedTags = new List<string>
            {
                path.ToString(),
                level.ToString()
            },
            Topics = new List<RoadmapTopicDraftDto>
            {
                new()
                {
                    Title = "Fundamentals",
                    Description = $"Learn the core fundamentals of {path}.",
                    SubTopics = new List<RoadmapSubTopicDraftDto>
                    {
                        new()
                        {
                            Title = "Core Concepts",
                            Description = "Understand the basic concepts before moving forward.",
                            Resources = new List<RoadmapResourceDraftDto>
                            {
                                new()
                                {
                                    Title = "Official Documentation",
                                    Type = RoadmapResourceType.Documentation,
                                    SearchKeyword = $"{path} {level} official documentation"
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    private static string NormalizeText(string? value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value)
            ? fallback.Trim()
            : value.Trim();
    }
}