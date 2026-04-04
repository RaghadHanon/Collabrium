using API.SetupProgram;
using Domain.Enums;

namespace Application.Common;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddAppAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            string[] TenantAdmins =
            {
                nameof(UserRole.SuperAdmin),
                nameof(UserRole.CEO),
                nameof(UserRole.TenantAdmin),
                nameof(UserRole.IT)
            };

            string[] LocalTenantAdmins =
            {
                nameof(UserRole.CEO),
                nameof(UserRole.TenantAdmin),
                nameof(UserRole.IT)
            };

            string[] ProductManagers =
            {
                nameof(UserRole.ProductOwner),
                nameof(UserRole.TeamLead),
                nameof(UserRole.BusinessAnalyst)
            };

            string[] DevRoles =
            {
                nameof(UserRole.BackendDev),
                nameof(UserRole.FrontendDev),
                nameof(UserRole.FullStackDev)
            };

            string[] ProductMembers =
            {
                nameof(UserRole.BackendDev),
                nameof(UserRole.FrontendDev),
                nameof(UserRole.FullStackDev),
                nameof(UserRole.QA)
            };

            string[] TenantMembers =
            {
                nameof(UserRole.TenantAdmin),
                nameof(UserRole.IT),
                nameof(UserRole.CEO),
                nameof(UserRole.HR),
                nameof(UserRole.Designer),

                nameof(UserRole.ProductOwner),
                nameof(UserRole.TeamLead),
                nameof(UserRole.BusinessAnalyst),

                nameof(UserRole.BackendDev),
                nameof(UserRole.FrontendDev),
                nameof(UserRole.FullStackDev),
                nameof(UserRole.QA)
            };

            options.AddPolicy(Policies.SuperAdmin, p => p.RequireRole(nameof(UserRole.SuperAdmin)));
            options.AddPolicy(Policies.TenantAdmin, p => p.RequireRole(nameof(UserRole.TenantAdmin)));
            options.AddPolicy(Policies.IT, p => p.RequireRole(nameof(UserRole.IT)));
            options.AddPolicy(Policies.CEO, p => p.RequireRole(nameof(UserRole.CEO)));
            options.AddPolicy(Policies.HR, p => p.RequireRole(nameof(UserRole.HR)));
            options.AddPolicy(Policies.ProductOwner, p => p.RequireRole(nameof(UserRole.ProductOwner)));
            options.AddPolicy(Policies.TeamLead, p => p.RequireRole(nameof(UserRole.TeamLead)));
            options.AddPolicy(Policies.BackendDev, p => p.RequireRole(nameof(UserRole.BackendDev)));
            options.AddPolicy(Policies.FrontendDev, p => p.RequireRole(nameof(UserRole.FrontendDev)));
            options.AddPolicy(Policies.FullStackDev, p => p.RequireRole(nameof(UserRole.FullStackDev)));
            options.AddPolicy(Policies.QA, p => p.RequireRole(nameof(UserRole.QA)));
            options.AddPolicy(Policies.BusinessAnalyst, p => p.RequireRole(nameof(UserRole.BusinessAnalyst)));
            options.AddPolicy(Policies.Designer, p => p.RequireRole(nameof(UserRole.Designer)));

            options.AddPolicy(Policies.PasswordChanged, policy =>
                policy.Requirements.Add(new IsPasswordChangedRequirement()));
            options.AddPolicy(Policies.Project_Access, policy =>
                policy.Requirements.Add(new MustHaveProjectAccessRequirement()));

            //Tenant
            options.AddPolicy(Policies.Tenant_Read, p => p.RequireRole(TenantMembers));
            options.AddPolicy(Policies.Tenant_Manage, p => p.RequireRole(TenantAdmins));
            options.AddPolicy(Policies.Local_Tenant_Manage, p => p.RequireRole(LocalTenantAdmins));

            //Projects
            options.AddPolicy(Policies.Projects_Read, p => p.RequireRole(TenantMembers));
            options.AddPolicy(Policies.High_Level_Manage, p => p.RequireRole(
                nameof(UserRole.CEO),
                nameof(UserRole.TenantAdmin)));
            options.AddPolicy(Policies.Projects_Manage, p => p.RequireRole(
                nameof(UserRole.ProductOwner),
                nameof(UserRole.TeamLead),
                nameof(UserRole.BusinessAnalyst),
                nameof(UserRole.TenantAdmin)
            ));

            //Teams
            options.AddPolicy(Policies.Teams_Read, p => p.RequireRole(TenantMembers));
            options.AddPolicy(Policies.Teams_Manage, p => p.RequireRole(
                nameof(UserRole.ProductOwner),
                nameof(UserRole.TeamLead),
                nameof(UserRole.BusinessAnalyst),
                nameof(UserRole.TenantAdmin)
            ));

            //Sprints
            options.AddPolicy(Policies.Sprints_Read, p => p.RequireRole(TenantMembers));
            options.AddPolicy(Policies.Sprints_Manage, p => p.RequireRole(
                nameof(UserRole.ProductOwner),
                nameof(UserRole.TeamLead),
                nameof(UserRole.BusinessAnalyst),
                nameof(UserRole.TenantAdmin)
            ));

            //WorkItems
            options.AddPolicy(Policies.WorkItems_Read, p => p.RequireRole(TenantMembers));
            options.AddPolicy(Policies.WorkItems_CreatePlanning, p => p.RequireRole(
                nameof(UserRole.ProductOwner),
                nameof(UserRole.TeamLead),
                nameof(UserRole.BusinessAnalyst),
                nameof(UserRole.TenantAdmin)
            ));
            options.AddPolicy(Policies.WorkItems_CreateExecution, p => p.RequireRole(
                nameof(UserRole.BackendDev),
                nameof(UserRole.FrontendDev),
                nameof(UserRole.FullStackDev),
                nameof(UserRole.QA),
                nameof(UserRole.TeamLead),
                nameof(UserRole.TenantAdmin)
            ));
            options.AddPolicy(Policies.WorkItems_UpdateContent, p => p.RequireRole(TenantMembers));
            options.AddPolicy(Policies.WorkItems_UpdatePlanning, p => p.RequireRole(
                nameof(UserRole.ProductOwner),
                nameof(UserRole.TeamLead),
                nameof(UserRole.BusinessAnalyst),
                nameof(UserRole.TenantAdmin)
            ));
            options.AddPolicy(Policies.WorkItems_UpdateExecution, p => p.RequireRole(
                nameof(UserRole.BackendDev),
                nameof(UserRole.FrontendDev),
                nameof(UserRole.FullStackDev),
                nameof(UserRole.QA),
                nameof(UserRole.TeamLead),
                nameof(UserRole.TenantAdmin)
            ));
            options.AddPolicy(Policies.WorkItems_AssignSelf, p => p.RequireRole(ProductMembers));
            options.AddPolicy(Policies.WorkItems_AssignOthers, p => p.RequireRole(
                nameof(UserRole.ProductOwner),
                nameof(UserRole.TeamLead),
                nameof(UserRole.TenantAdmin)
            ));
            options.AddPolicy(Policies.WorkItems_Delete, p => p.RequireRole(
                nameof(UserRole.ProductOwner),
                nameof(UserRole.TeamLead),
                nameof(UserRole.TenantAdmin)
            ));
            options.AddPolicy(Policies.WorkItems_ChangeStatus, p => p.RequireRole(TenantMembers));
            options.AddPolicy(Policies.WorkItems_ChangeAssignee, p => p.RequireRole(TenantMembers));

            //Roadmaps
            options.AddPolicy(Policies.Roadmaps_Read, p => p.RequireRole(TenantMembers));
            options.AddPolicy(Policies.Roadmaps_Manage, p => p.RequireRole(
                nameof(UserRole.ProductOwner),
                nameof(UserRole.TeamLead),
                nameof(UserRole.BusinessAnalyst),
                nameof(UserRole.TenantAdmin)
            ));

            //Tags
            options.AddPolicy(Policies.Tags_Read, p => p.RequireRole(TenantMembers));
            options.AddPolicy(Policies.Tags_Manage, p => p.RequireRole(
                nameof(UserRole.ProductOwner),
                nameof(UserRole.TeamLead),
                nameof(UserRole.BusinessAnalyst),
                nameof(UserRole.TenantAdmin)
            ));
        });

        return services;
    }
}