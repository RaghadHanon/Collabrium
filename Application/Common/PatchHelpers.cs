namespace Application.Common;

internal static class PatchHelpers
{
    public enum PatchStringAction { Ignore, Clear, Set }

    public readonly record struct PatchStringDecision(PatchStringAction Action, string? Value);

    public static PatchStringDecision DecideStringPatch(string? input)
    {
        if (input is null)
            return new PatchStringDecision(PatchStringAction.Ignore, null);

        if (string.IsNullOrWhiteSpace(input))
            return new PatchStringDecision(PatchStringAction.Clear, null);

        return new PatchStringDecision(PatchStringAction.Set, input.Trim());
    }

    public static string? ApplyStringPatch(string? current, string? input)
    {
        var d = DecideStringPatch(input);
        return d.Action switch
        {
            PatchStringAction.Ignore => current,
            PatchStringAction.Clear  => null,
            PatchStringAction.Set    => d.Value,
            _ => current
        };
    }
}