using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace DFC.ServiceTaxonomy.Cypher.Menus
{
    public class AdminMenu : INavigationProvider
    {
        private readonly IStringLocalizer S;

        public AdminMenu(IStringLocalizer<AdminMenu> localizer) => S = localizer;

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!string.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder
                .Add(S["Search"], search => search
                    .Add(S["Queries"], "10", queries => queries
                        .Add(S["Run Cypher Query"], "5", cypher => cypher
                            .Action(nameof(Controllers.AdminController.RunQuery), "Admin", new { area = typeof(Startup).Namespace })
                            .Permission(Permissions.Permissions.ManageCypherQueries)
                            .LocalNav())));

            return Task.CompletedTask;
        }
    }
}
