using Application.Features.AI.DTOs.WorkItems;
using Application.Interfaces.AI;
using Domain.Enums;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Infrastructure.AI;

public sealed class WorkItemFieldsSuggestionService(
    IAiAssistantService aiAssistantService)
    : IWorkItemFieldsSuggestionService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    private static readonly int[] AllowedStoryPoints = [1, 2, 3, 5, 8, 13];

    public async Task<TSuggestion> GenerateAsync<TSuggestion>(
        WorkItemType workItemType,
        string summary,
        CancellationToken cancellationToken = default)
        where TSuggestion : WorkItemFieldsSuggestionDto
    {
        if (string.IsNullOrWhiteSpace(summary))
            throw new ArgumentException("Summary is required.");

        var prompt = BuildPrompt(workItemType, summary.Trim());

        var aiResponse = await aiAssistantService.AskAsync(prompt, cancellationToken);

        var json = ExtractJson(aiResponse);

        var suggestion = DeserializeSuggestion<TSuggestion>(json);

        ValidateSuggestion(workItemType, suggestion);

        return suggestion;
    }

    private static TSuggestion DeserializeSuggestion<TSuggestion>(string json)
        where TSuggestion : WorkItemFieldsSuggestionDto
    {
        try
        {
            var suggestion = JsonSerializer.Deserialize<TSuggestion>(json, JsonOptions);

            return suggestion ?? throw new InvalidOperationException(
                "AI returned an empty or invalid suggestion.");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"AI returned JSON in an invalid format. Details: {ex.Message}");
        }
    }

    private static string BuildPrompt(WorkItemType workItemType, string summary)
    {
        var schema = GetJsonSchema(workItemType);

        var priorityValues = string.Join(", ", Enum.GetNames<WorkItemPriority>());
        var testEnvironmentValues = string.Join(", ", Enum.GetNames<TestEnvironment>());

        return $$"""
        You are an AI assistant inside Collabrium.

        Your task is to generate suggested fields for creating a {{workItemType}} work item.

        User summary:
        {{summary}}

        Strict rules:
        - Return valid JSON only.
        - Do not wrap the JSON in markdown.
        - Do not add explanations before or after the JSON.
        - Use only the fields shown in the required JSON schema.
        - Do not invent IDs.
        - Do not include projectId, createdByUserId, status, code, sprintId, parentId, startDate, or endDate.
        - title must be concise and clear.
        - description must be practical and specific.
        - acceptanceCriteria must be clear, testable, and written as completion conditions.
        - effortPoints must be a non-negative integer.
        - priority must be exactly one of these string values: {{priorityValues}}.
        - Do not return enum values as numbers.
        - If the summary is vague, provide reasonable but conservative suggestions.

        Type-specific rules:
        - For UserStory, storyPoints must be exactly one of: 1, 2, 3, 5, 8, 13.
        - For Bug, stepsToReproduce, expectedResult, and actualResult are required.
        - For TestCase, preconditions, steps, testEnvironment, and expectedResult are required.
        - For TestCase, testEnvironment must be exactly one of these string values: {{testEnvironmentValues}}.

        Required JSON schema:
        {{schema}}
        """;
    }

    private static string GetJsonSchema(WorkItemType workItemType)
    {
        return workItemType switch
        {
            WorkItemType.Epic =>
                """
                {
                  "title": "string",
                  "description": "string",
                  "acceptanceCriteria": "string",
                  "effortPoints": 0,
                  "priority": "Medium"
                }
                """,

            WorkItemType.UserStory =>
                """
                {
                  "title": "string",
                  "description": "string",
                  "acceptanceCriteria": "string",
                  "effortPoints": 0,
                  "priority": "Medium",
                  "storyPoints": 3
                }
                """,

            WorkItemType.TaskItem =>
                """
                {
                  "title": "string",
                  "description": "string",
                  "acceptanceCriteria": "string",
                  "effortPoints": 0,
                  "priority": "Medium"
                }
                """,

            WorkItemType.Bug =>
                """
                {
                  "title": "string",
                  "description": "string",
                  "acceptanceCriteria": "string",
                  "effortPoints": 0,
                  "priority": "High",
                  "stepsToReproduce": "string",
                  "expectedResult": "string",
                  "actualResult": "string"
                }
                """,

            WorkItemType.TestCase =>
                $$"""
                {
                  "title": "string",
                  "description": "string",
                  "acceptanceCriteria": "string",
                  "effortPoints": 0,
                  "priority": "Medium",
                  "preconditions": "string",
                  "steps": "string",
                  "testEnvironment": "{{GetDefaultTestEnvironmentValue()}}",
                  "expectedResult": "string"
                }
                """,

            _ => throw new NotSupportedException(
                $"AI field suggestion is not supported for work item type '{workItemType}'.")
        };
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

        return !jsonMatch.Success
            ? throw new InvalidOperationException("AI response does not contain a valid JSON object.")
            : jsonMatch.Value.Trim();
    }

    private static void ValidateSuggestion<TSuggestion>(
        WorkItemType workItemType,
        TSuggestion suggestion)
        where TSuggestion : WorkItemFieldsSuggestionDto
    {
        ValidateCommonFields(suggestion);

        switch (workItemType)
        {
            case WorkItemType.UserStory:
                ValidateUserStorySuggestion(suggestion);
                break;

            case WorkItemType.Bug:
                ValidateBugSuggestion(suggestion);
                break;

            case WorkItemType.TestCase:
                ValidateTestCaseSuggestion(suggestion);
                break;

            case WorkItemType.Epic:
            case WorkItemType.TaskItem:
                break;

            default:
                throw new NotSupportedException(
                    $"AI field suggestion is not supported for work item type '{workItemType}'.");
        }
    }

    private static void ValidateCommonFields(WorkItemFieldsSuggestionDto suggestion)
    {
        if (string.IsNullOrWhiteSpace(suggestion.Title))
            throw new InvalidOperationException("AI suggestion must include a title.");

        if (suggestion.EffortPoints < 0)
            throw new InvalidOperationException("EffortPoints cannot be negative.");

        if (!Enum.IsDefined(typeof(WorkItemPriority), suggestion.Priority))
            throw new InvalidOperationException("AI suggestion contains an invalid priority.");
    }

    private static void ValidateUserStorySuggestion<TSuggestion>(TSuggestion suggestion)
        where TSuggestion : WorkItemFieldsSuggestionDto
    {
        if (suggestion is not UserStoryFieldsSuggestionDto userStory)
            throw new InvalidOperationException("AI returned an invalid User Story suggestion format.");

        if (!AllowedStoryPoints.Contains(userStory.StoryPoints))
        {
            throw new InvalidOperationException(
                "StoryPoints must be one of: 1, 2, 3, 5, 8, 13.");
        }
    }

    private static void ValidateBugSuggestion<TSuggestion>(TSuggestion suggestion)
        where TSuggestion : WorkItemFieldsSuggestionDto
    {
        if (suggestion is not BugFieldsSuggestionDto bug)
            throw new InvalidOperationException("AI returned an invalid Bug suggestion format.");

        if (string.IsNullOrWhiteSpace(bug.StepsToReproduce))
            throw new InvalidOperationException("StepsToReproduce is required.");

        if (string.IsNullOrWhiteSpace(bug.ExpectedResult))
            throw new InvalidOperationException("ExpectedResult is required.");

        if (string.IsNullOrWhiteSpace(bug.ActualResult))
            throw new InvalidOperationException("ActualResult is required.");
    }

    private static void ValidateTestCaseSuggestion<TSuggestion>(TSuggestion suggestion)
        where TSuggestion : WorkItemFieldsSuggestionDto
    {
        if (suggestion is not TestCaseFieldsSuggestionDto testCase)
            throw new InvalidOperationException("AI returned an invalid Test Case suggestion format.");

        if (string.IsNullOrWhiteSpace(testCase.Preconditions))
            throw new InvalidOperationException("Preconditions is required.");

        if (string.IsNullOrWhiteSpace(testCase.Steps))
            throw new InvalidOperationException("Steps is required.");

        if (string.IsNullOrWhiteSpace(testCase.ExpectedResult))
            throw new InvalidOperationException("ExpectedResult is required.");

        if (!Enum.IsDefined(typeof(TestEnvironment), testCase.TestEnvironment))
            throw new InvalidOperationException("AI suggestion contains an invalid test environment.");
    }

    private static string GetDefaultTestEnvironmentValue()
    {
        var names = Enum.GetNames<TestEnvironment>();

        return names.Contains("Software")
            ? "Software"
            : names.FirstOrDefault()
            ?? throw new InvalidOperationException("TestEnvironment enum has no defined values.");
    }
}