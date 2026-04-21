namespace API.SetupProgram;

public static class Policies
{
    public const string SuperAdmin = "SuperAdmin";
    public const string TenantAdmin = "TenantAdmin";
    public const string IT = "IT";
    public const string CEO = "CEO";
    public const string HR = "HR";
    public const string ProductOwner = "ProductOwner";
    public const string TeamLead = "TeamLead";
    public const string BackendDev = "BackendDev";
    public const string FrontendDev = "FrontendDev";
    public const string FullStackDev = "FullStackDev";
    public const string QA = "QA";
    public const string BusinessAnalyst = "BusinessAnalyst";
    public const string Designer = "Designer";

    public const string PasswordChanged = "PasswordChanged";
    public const string Project_Access = "Project_Access";

    // Tenant
    public const string Tenant_Read = "Tenant.Read";
    public const string Tenant_Manage = "Tenant.Manage";
    public const string Local_Tenant_Manage = "Local.Tenant.Manage";

    // Projects
    public const string Projects_Read = "Projects.Read";
    public const string High_Level_Manage = "HighLevelManage";
    public const string Projects_Manage = "Projects.Manage";

    // Teams
    public const string Teams_Read = "Teams.Read";
    public const string Teams_Manage = "Teams.Manage";

    // Sprints
    public const string Sprints_Read = "Sprints.Read";
    public const string Sprints_Manage = "Sprints.Manage";

    // WorkItems
    public const string WorkItems_Read = "WorkItems.Read";

    public const string WorkItems_CreatePlanning = "WorkItems.Create.Planning";     // Epic/Story
    public const string WorkItems_CreateExecution = "WorkItems.Create.Execution";   // Task/Bug/TestCase

    public const string WorkItems_UpdateContent = "WorkItems.Update.Content";
    public const string WorkItems_UpdatePlanning = "WorkItems.Update.Planning";
    public const string WorkItems_UpdateExecution = "WorkItems.Update.Execution";

    public const string WorkItems_AssignSelf = "WorkItems.Assign.Self";
    public const string WorkItems_AssignOthers = "WorkItems.Assign.Others";

    public const string WorkItems_Delete = "WorkItems.Delete";
    public const string WorkItems_ChangeStatus = "WorkItems.ChangeStatusWithValidation";
    public const string WorkItems_ChangeAssignee = "WorkItems.ChangeAssignee";

    // Roadmaps
    public const string Roadmaps_Read = "Roadmaps.Read";
    public const string Roadmaps_Manage = "Roadmaps.Manage";

    //Tags
    public const string Tags_Read = "Tags.Read";
    public const string Tags_Manage = "Tags.Manage";

    //Courses
    public const string Courses_Read = "Courses.Read";
    public const string Courses_Manage = "Courses.Manage";
}