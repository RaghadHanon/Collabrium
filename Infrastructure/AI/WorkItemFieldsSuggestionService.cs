using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Application.Features.AI.DTOs.WorkItems;
using Application.Interfaces.AI;
using Domain.Enums;

namespace Infrastructure.AI;

public sealed class WorkItemFieldsSuggestionService(
    IAiAssistantService aiAssistantService)
    : IWorkItemFieldsSuggestionService
{
    private const int MaxAttempts = 3;

    private static readonly int[] AllowedStoryPoints = [1, 2, 3, 5, 8, 13];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    public async Task<TSuggestion> GenerateAsync<TSuggestion>(
        WorkItemType workItemType,
        string summary,
        CancellationToken cancellationToken = default)
        where TSuggestion : WorkItemFieldsSuggestionDto
    {
        if (string.IsNullOrWhiteSpace(summary))
            throw new ArgumentException("Summary is required.");

        summary = summary.Trim();

        var prompt = BuildInitialPrompt(workItemType, summary);
        string? lastError = null;
        string? lastResponse = null;

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            var aiResponse = await aiAssistantService.AskAsync(prompt, cancellationToken);
            lastResponse = aiResponse;

            var parsed = TryParseSuggestion<TSuggestion>(
                workItemType,
                aiResponse,
                summary,
                out var suggestion,
                out var error);

            if (parsed && suggestion is not null)
                return suggestion;

            lastError = error;

            prompt = BuildRepairPrompt(
                workItemType,
                summary,
                aiResponse,
                error ?? "The response was invalid.");
        }

        return BuildFallbackSuggestion<TSuggestion>(
            workItemType,
            summary,
            lastError,
            lastResponse);
    }

    private static bool TryParseSuggestion<TSuggestion>(
        WorkItemType workItemType,
        string aiResponse,
        string summary,
        out TSuggestion? suggestion,
        out string? error)
        where TSuggestion : WorkItemFieldsSuggestionDto
    {
        suggestion = null;
        error = null;

        try
        {
            var json = ExtractJson(aiResponse);

            suggestion = JsonSerializer.Deserialize<TSuggestion>(
                json,
                JsonOptions);

            if (suggestion is null)
            {
                error = "The JSON object could not be deserialized.";
                return false;
            }

            NormalizeSuggestion(workItemType, suggestion, summary);

            if (!IsSuggestionUsable(workItemType, suggestion, out error))
                return false;

            return true;
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            error = ex.Message;
            return false;
        }
    }

    private static string BuildInitialPrompt(
        WorkItemType workItemType,
        string summary)
    {
        var schema = GetJsonSchema(workItemType);
        var priorityValues = string.Join(", ", Enum.GetNames<WorkItemPriority>());
        var testEnvironmentValues = string.Join(", ", Enum.GetNames<TestEnvironment>());

        return $$"""
        You are an AI assistant inside Collabrium.

        Generate suggested fields for creating a {{workItemType}} work item.

        User summary:
        {{summary}}

        Rules:
        - Return valid JSON only.
        - Do not wrap the JSON in markdown.
        - Do not add explanations.
        - Use only the fields shown in the required JSON schema.
        - Do not include IDs or system fields.
        - Do not include projectId, createdByUserId, status, code, sprintId, parentId, startDate, or endDate.
        - Do not leave required fields empty.
        - priority must be exactly one of these string values: {{priorityValues}}.
        - Do not return enum values as numbers.
        - effortPoints must be a non-negative integer.

        Type-specific rules:
        - For UserStory, storyPoints must be one of: 1, 2, 3, 5, 8, 13.
        - For Bug, stepsToReproduce, expectedResult, and actualResult must be non-empty strings.
        - For Bug, if actualResult is not explicitly stated, infer it from the summary.
        - For Bug, if reproduction steps are not explicit, create reasonable steps based on the summary.
        - For TestCase, preconditions, steps, testEnvironment, and expectedResult must be non-empty strings.
        - For TestCase, testEnvironment must be exactly one of these string values: {{testEnvironmentValues}}.

        Required JSON schema:
        {{schema}}
        """;
    }

    private static string BuildRepairPrompt(
        WorkItemType workItemType,
        string summary,
        string invalidResponse,
        string error)
    {
        var schema = GetJsonSchema(workItemType);
        var priorityValues = string.Join(", ", Enum.GetNames<WorkItemPriority>());
        var testEnvironmentValues = string.Join(", ", Enum.GetNames<TestEnvironment>());

        return $$"""
        Your previous response was invalid for creating a {{workItemType}} work item.

        Validation/parsing problem:
        {{error}}

        User summary:
        {{summary}}

        Previous invalid response:
        {{invalidResponse}}

        Fix the response now.

        Rules:
        - Return valid JSON only.
        - Do not wrap the JSON in markdown.
        - Do not add explanations.
        - Use only the fields shown in the schema.
        - Do not leave required fields empty.
        - priority must be exactly one of: {{priorityValues}}.
        - Do not return enum values as numbers.
        - For UserStory, storyPoints must be one of: 1, 2, 3, 5, 8, 13.
        - For Bug, stepsToReproduce, expectedResult, and actualResult must be non-empty strings.
        - For TestCase, preconditions, steps, testEnvironment, and expectedResult must be non-empty strings.
        - For TestCase, testEnvironment must be exactly one of: {{testEnvironmentValues}}.

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

        if (!jsonMatch.Success)
            throw new InvalidOperationException("AI response does not contain a valid JSON object.");

        return jsonMatch.Value.Trim();
    }

    private static void NormalizeSuggestion<TSuggestion>(
        WorkItemType workItemType,
        TSuggestion suggestion,
        string summary)
        where TSuggestion : WorkItemFieldsSuggestionDto
    {
        suggestion.Title = NormalizeText(
            suggestion.Title,
            BuildFallbackTitle(workItemType, summary));

        suggestion.Description = NormalizeText(
            suggestion.Description,
            summary);

        suggestion.AcceptanceCriteria = NormalizeText(
            suggestion.AcceptanceCriteria,
            "The work item is complete when the described behavior is implemented, verified, and accepted.");

        if (suggestion.EffortPoints < 0)
            suggestion.EffortPoints = 0;

        if (!Enum.IsDefined(typeof(WorkItemPriority), suggestion.Priority))
            suggestion.Priority = WorkItemPriority.Medium;

        switch (workItemType)
        {
            case WorkItemType.UserStory when suggestion is UserStoryFieldsSuggestionDto userStory:
                if (!AllowedStoryPoints.Contains(userStory.StoryPoints))
                    userStory.StoryPoints = 3;
                break;

            case WorkItemType.Bug when suggestion is BugFieldsSuggestionDto bug:
                bug.StepsToReproduce = NormalizeText(
                    bug.StepsToReproduce,
                    BuildFallbackBugSteps(summary));

                bug.ExpectedResult = NormalizeText(
                    bug.ExpectedResult,
                    "The feature should behave correctly without errors or unexpected behavior.");

                bug.ActualResult = NormalizeText(
                    bug.ActualResult,
                    $"The issue described in the summary occurs: {summary}");

                break;

            case WorkItemType.TestCase when suggestion is TestCaseFieldsSuggestionDto testCase:
                testCase.Preconditions = NormalizeText(
                    testCase.Preconditions,
                    "The user has access to the system and the related feature is available.");

                testCase.Steps = NormalizeText(
                    testCase.Steps,
                    BuildFallbackTestSteps(summary));

                testCase.ExpectedResult = NormalizeText(
                    testCase.ExpectedResult,
                    "The system should produce the expected behavior described in the summary.");

                if (!Enum.IsDefined(typeof(TestEnvironment), testCase.TestEnvironment))
                    testCase.TestEnvironment = GetDefaultTestEnvironment();

                break;
        }
    }

    private static bool IsSuggestionUsable<TSuggestion>(
        WorkItemType workItemType,
        TSuggestion suggestion,
        out string? error)
        where TSuggestion : WorkItemFieldsSuggestionDto
    {
        error = null;

        if (string.IsNullOrWhiteSpace(suggestion.Title))
        {
            error = "Missing title.";
            return false;
        }

        if (suggestion.EffortPoints < 0)
        {
            error = "Invalid effortPoints.";
            return false;
        }

        if (!Enum.IsDefined(typeof(WorkItemPriority), suggestion.Priority))
        {
            error = "Invalid priority.";
            return false;
        }

        return workItemType switch
        {
            WorkItemType.Epic or WorkItemType.TaskItem => true,

            WorkItemType.UserStory =>
                suggestion is UserStoryFieldsSuggestionDto userStory &&
                AllowedStoryPoints.Contains(userStory.StoryPoints),

            WorkItemType.Bug =>
                suggestion is BugFieldsSuggestionDto bug &&
                !string.IsNullOrWhiteSpace(bug.StepsToReproduce) &&
                !string.IsNullOrWhiteSpace(bug.ExpectedResult) &&
                !string.IsNullOrWhiteSpace(bug.ActualResult),

            WorkItemType.TestCase =>
                suggestion is TestCaseFieldsSuggestionDto testCase &&
                !string.IsNullOrWhiteSpace(testCase.Preconditions) &&
                !string.IsNullOrWhiteSpace(testCase.Steps) &&
                !string.IsNullOrWhiteSpace(testCase.ExpectedResult) &&
                Enum.IsDefined(typeof(TestEnvironment), testCase.TestEnvironment),

            _ => false
        };
    }

    private static TSuggestion BuildFallbackSuggestion<TSuggestion>(
        WorkItemType workItemType,
        string summary,
        string? lastError,
        string? lastResponse)
        where TSuggestion : WorkItemFieldsSuggestionDto
    {
        WorkItemFieldsSuggestionDto suggestion = workItemType switch
        {
            WorkItemType.Epic => new EpicFieldsSuggestionDto(),

            WorkItemType.UserStory => new UserStoryFieldsSuggestionDto
            {
                StoryPoints = 3
            },

            WorkItemType.TaskItem => new TaskFieldsSuggestionDto(),

            WorkItemType.Bug => new BugFieldsSuggestionDto
            {
                StepsToReproduce = BuildFallbackBugSteps(summary),
                ExpectedResult = "The feature should behave correctly without errors or unexpected behavior.",
                ActualResult = $"The issue described in the summary occurs: {summary}"
            },

            WorkItemType.TestCase => new TestCaseFieldsSuggestionDto
            {
                Preconditions = "The user has access to the system and the related feature is available.",
                Steps = BuildFallbackTestSteps(summary),
                TestEnvironment = GetDefaultTestEnvironment(),
                ExpectedResult = "The system should produce the expected behavior described in the summary."
            },

            _ => throw new NotSupportedException(
                $"AI field suggestion is not supported for work item type '{workItemType}'.")
        };

        suggestion.Title = BuildFallbackTitle(workItemType, summary);
        suggestion.Description = summary;
        suggestion.AcceptanceCriteria =
            "The work item is complete when the described behavior is implemented, verified, and accepted.";
        suggestion.EffortPoints = 1;
        suggestion.Priority = WorkItemPriority.Medium;

        return suggestion is TSuggestion typedSuggestion
            ? typedSuggestion
            : throw new InvalidOperationException(
                $"Could not build fallback suggestion for {typeof(TSuggestion).Name}.");
    }

    private static string NormalizeText(string? value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value)
            ? fallback
            : value.Trim();
    }

    private static string BuildFallbackTitle(
        WorkItemType workItemType,
        string summary)
    {
        var normalized = summary.Trim();

        if (normalized.Length > 70)
            normalized = normalized[..70].TrimEnd() + "...";

        return workItemType switch
        {
            WorkItemType.Epic => $"Epic: {normalized}",
            WorkItemType.UserStory => $"User Story: {normalized}",
            WorkItemType.TaskItem => $"Task: {normalized}",
            WorkItemType.Bug => $"Bug: {normalized}",
            WorkItemType.TestCase => $"Test Case: {normalized}",
            _ => normalized
        };
    }

    private static string BuildFallbackBugSteps(string summary)
    {
        return
            $"1. Open the related feature.\n" +
            $"2. Perform the action described in the summary.\n" +
            $"3. Observe the reported issue.\n\n" +
            $"Summary: {summary}";
    }

    private static string BuildFallbackTestSteps(string summary)
    {
        return
            $"1. Open the related feature.\n" +
            $"2. Execute the scenario described in the summary.\n" +
            $"3. Verify the result.\n\n" +
            $"Summary: {summary}";
    }

    private static string GetDefaultTestEnvironmentValue()
    {
        return GetDefaultTestEnvironment().ToString();
    }

    private static TestEnvironment GetDefaultTestEnvironment()
    {
        var values = Enum.GetValues<TestEnvironment>();

        if (values.Contains(TestEnvironment.Software))
            return TestEnvironment.Software;

        return values.FirstOrDefault();
    }
}