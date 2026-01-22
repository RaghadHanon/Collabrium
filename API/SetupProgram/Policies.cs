namespace API.SetupProgram;

/// <summary>
/// Centralized policy names to avoid magic strings.
/// </summary>
public static class Policies
{
    public const string SuperAdmin = "IsSuperAdmin";
    public const string CEO = "IsCEO";
    public const string HR = "IsHR";
    public const string ProductOwner = "IsProductOwner";
    public const string TeamLead = "IsTeamLead";
    public const string BackendDev = "IsBackendDev";
    public const string FrontendDev = "IsFrontendDev";
    public const string FullStackDev = "IsFullStackDev";
    public const string QA = "IsQA";
    public const string BusinessAnalyst = "IsBusinessAnalyst";
    public const string Designer = "IsDesigner";
    public const string IT = "IsIT";
    public const string TenantAdmin = "TenantAdmin";
    public const string PasswordChanged = "IsPasswordChanged";
    public const string ProductTeamManagerOrTenantAdmin = "ProductTeamManagerOrTenantAdmin";
    public const string ProductTeamManager = "ProductTeamManager";
    public const string ProductTeamMember = "ProductTeamMember";
}
