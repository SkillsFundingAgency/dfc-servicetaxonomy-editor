using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace DFC.ServiceTaxonomy.GraphSync
{
    public class AdminMenu : INavigationProvider
    {
        private readonly IStringLocalizer<AdminMenu> S;

        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            S = localizer;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!string.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
                return Task.CompletedTask;

            builder
                .Add(S["Graph"], "99", graph => graph
                .AddClass("graph").Id("graph")
                    .Add(S["Validate and Repair All Items"], "2", validateAndRepair => validateAndRepair
                        .Permission(Permissions.AdministerGraphs)
                        .Action("TriggerSyncValidation", "GraphSync", new
                        {
                            area = typeof(Startup)!.Namespace,
                            scope = ValidationScope.AllItems
                        }))
                    .Add(S["Validate and Repair Recent Items"], "3", validateAndRepair => validateAndRepair
                        .Permission(Permissions.AdministerGraphs)
                        .Action("TriggerSyncValidation", "GraphSync", new
                        {
                            area = typeof(Startup)!.Namespace,
                            scope = ValidationScope.ModifiedSinceLastValidation
                        }))
                .Add(S["Dump Control"], "4", dumpControl => dumpControl
                    .Permission(Permissions.AdministerGraphs)
                    .Action("DumpControl", "GraphSync", new
                    {
                        area = typeof(Startup)!.Namespace
                    }))
                );

            return Task.CompletedTask;
        }
    }
}
