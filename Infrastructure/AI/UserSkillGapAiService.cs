using Application.Features.AI.DTOs.SkillGaps;
using Application.Interfaces.AI;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Infrastructure.AI;
public sealed class UserSkillGapAiService(
    IAiAssistantService aiAssistantService)
    : IUserSkillGapAiService
{
    private const int MaxAttempts = 2;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public Task<SkillGapAnalysisDto> AnalyzeAsync(
        string role,
        IReadOnlyCollection<string> userSkills,
        CancellationToken cancellationToken = default)
    {
        var prompt = $$"""
        You are an AI learning and career assistant inside Collabrium.

        Analyze the user's current skills based on their role.

        User Role:
        {{role}}

        Current Skills:
        {{FormatList(userSkills)}}

        Tasks:
        1. Compare the user's current skills with the role requirements.
        2. Identify strengths from the existing skills.
        3. Identify the most important missing skills.
        4. Give practical advice without recommending courses or roadmaps.

        Rules:
        - Return valid JSON only.
        - Do not use markdown.
        - Do not mention IDs.
        - Do not recommend courses.
        - Do not recommend roadmaps.
        - Summary must be 1 short sentence.
        - ReadinessScore must be between 0 and 100.
        - ReadinessLevel must be one of: Beginner, Intermediate, Advanced.
        - Return exactly 3 strengths.
        - Return exactly 3 missing skills.
        - Each missing skill description must be one short sentence.
        - whyItMatters must be one short sentence.
        - recommendedLevel must be one of: Beginner, Intermediate, Advanced.
        - FinalAdvice must be 1-2 short sentences.

        Return JSON using exactly this structure:

        {
          "role": "string",
          "readinessScore": 0,
          "summary": "string",
          "strengths": ["string"],
          "missingSkills": [
            {
              "skill": "string",
              "description": "string",
              "whyItMatters": "string",
              "recommendedLevel": "string"
            }
          ],
          "topPrioritySkill": "string",
          "readinessLevel": "string",
          "finalAdvice": "string"
        }
        """;

        return GenerateAsync(
            prompt,
            BuildFallback(role, userSkills),
            cancellationToken);
    }

    private async Task<SkillGapAnalysisDto> GenerateAsync(
        string initialPrompt,
        SkillGapAnalysisDto fallback,
        CancellationToken cancellationToken)
    {
        var prompt = initialPrompt;

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            var aiResponse = await aiAssistantService.AskAsync(prompt, cancellationToken);

            if (TryParse(aiResponse, out var result) && result is not null)
            {
                Normalize(result);
                return result;
            }

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

    private static bool TryParse(string aiResponse, out SkillGapAnalysisDto? result)
    {
        result = null;

        try
        {
            var json = ExtractJson(aiResponse);

            result = JsonSerializer.Deserialize<SkillGapAnalysisDto>(
                json,
                JsonOptions);

            return result is not null;
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

        var jsonMatch = Regex.Match(trimmed, @"\{[\s\S]*\}", RegexOptions.Multiline);

        if (!jsonMatch.Success)
            throw new InvalidOperationException("AI response does not contain JSON.");

        return jsonMatch.Value.Trim();
    }

    private static void Normalize(SkillGapAnalysisDto dto)
    {
        dto.Role = NormalizeText(dto.Role, "Software Developer");

        dto.Summary = NormalizeText(
            dto.Summary,
            "Your current skills were analyzed against your role requirements.");

        dto.FinalAdvice = NormalizeText(
            dto.FinalAdvice,
            "Focus on the most important missing skills and apply them in real projects.");

        dto.ReadinessScore = Math.Clamp(dto.ReadinessScore, 0, 100);

        dto.ReadinessLevel = NormalizeLevel(
            dto.ReadinessLevel,
            dto.ReadinessScore);

        dto.Strengths ??= new();
        dto.Strengths = dto.Strengths
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(3)
            .ToList();

        dto.MissingSkills ??= new();

        dto.MissingSkills = dto.MissingSkills
            .Where(x => !string.IsNullOrWhiteSpace(x.Skill))
            .Select(x =>
            {
                x.Skill = x.Skill.Trim();

                x.Description = NormalizeText(
                    x.Description,
                    "This skill is important for your target role.");

                x.WhyItMatters = NormalizeText(
                    x.WhyItMatters,
                    "It helps you perform better in this role.");

                x.RecommendedLevel = NormalizeSkillLevel(x.RecommendedLevel);

                return x;
            })
            .DistinctBy(x => x.Skill.ToLowerInvariant())
            .Take(3)
            .ToList();

        dto.TopPrioritySkill = NormalizeText(
            dto.TopPrioritySkill,
            dto.MissingSkills.FirstOrDefault()?.Skill ?? "Core role skills");
    }

    private static string NormalizeText(string? value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value)
            ? fallback
            : value.Trim();
    }

    private static string NormalizeLevel(string? value, int score)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            var level = value.Trim();

            if (level.Equals("Beginner", StringComparison.OrdinalIgnoreCase))
                return "Beginner";

            if (level.Equals("Intermediate", StringComparison.OrdinalIgnoreCase))
                return "Intermediate";

            if (level.Equals("Advanced", StringComparison.OrdinalIgnoreCase))
                return "Advanced";
        }

        return score switch
        {
            < 45 => "Beginner",
            < 75 => "Intermediate",
            _ => "Advanced"
        };
    }

    private static string NormalizeSkillLevel(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "Intermediate";

        var level = value.Trim();

        return level.Equals("Beginner", StringComparison.OrdinalIgnoreCase) ||
               level.Equals("Intermediate", StringComparison.OrdinalIgnoreCase) ||
               level.Equals("Advanced", StringComparison.OrdinalIgnoreCase)
            ? level
            : "Intermediate";
    }

    private static string FormatList(IReadOnlyCollection<string> items)
    {
        if (items is null || items.Count == 0)
            return "- None";

        return string.Join(
            Environment.NewLine,
            items
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => $"- {x.Trim()}")
                .Distinct(StringComparer.OrdinalIgnoreCase));
    }

    private static SkillGapAnalysisDto BuildFallback(
        string role,
        IReadOnlyCollection<string> userSkills)
    {
        var score = userSkills.Count > 0 ? 50 : 20;

        return new SkillGapAnalysisDto
        {
            Role = string.IsNullOrWhiteSpace(role)
                ? "Software Developer"
                : role.Trim(),

            Summary = "The AI could not generate a skill gap analysis right now.",
            ReadinessScore = score,
            ReadinessLevel = NormalizeLevel(null, score),

            Strengths = userSkills
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(3)
                .ToList(),

            MissingSkills = new(),

            TopPrioritySkill = "Core role skills",

            FinalAdvice = "Focus on the most important skills for your role and practice them in real projects."
        };
    }
}