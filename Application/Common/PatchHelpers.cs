namespace Application.Common;

internal static class PatchHelpers
{
    public enum PatchStringAction { Ignore, Clear, Set }

    public readonly record struct PatchStringDecision(PatchStringAction Action, string? Value);

    public static PatchStringDecision DecideStringPatch(string? input)
    {
        return input is null
            ? new PatchStringDecision(PatchStringAction.Ignore, null)
            : string.IsNullOrWhiteSpace(input)
            ? new PatchStringDecision(PatchStringAction.Clear, null)
            : new PatchStringDecision(PatchStringAction.Set, input.Trim());
    }

    public static string? ApplyStringPatch(string? current, string? input)
    {
        var d = DecideStringPatch(input);
        return d.Action switch
        {
            PatchStringAction.Ignore => current,
            PatchStringAction.Clear => null,
            PatchStringAction.Set => d.Value,
            _ => current
        };
    }
}