namespace API.SetupProgram;

/// <summary>
/// Centralized policy names to avoid magic strings.
/// </summary>
public static class Policies
{
    public const string SuperAdmin = "IsSuperAdmin";
    public const string CEO = "IsCEO";
    public const string HR = "IsHR";
    public const string ProductManager = "IsProductManager";
    public const string TeamLead = "IsTeamLead";
    public const string DevOps = "IsDevOps";
    public const string BackendDeveloper = "IsBackendDeveloper";
    public const string FrontendDeveloper = "IsFrontendDeveloper";
    public const string FullStackDeveloper = "IsFullStackDeveloper";
    public const string QA = "IsQA";
    public const string BusinessAnalyst = "IsBusinessAnalyst";
    public const string Designer = "IsDesigner";
}
