using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Localization;

using OrchardCore.Navigation;

namespace DFC.ServiceTaxonomy.JobProfiles.DataTransfer
{
    public class AdminMenu : INavigationProvider
    {
        private readonly IStringLocalizer S;

        public AdminMenu(IStringLocalizer<AdminMenu> localizer) =>
            S = localizer;

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (string.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                builder
                    .Add(S["Azure Search"], "99", azSearch =>
                        azSearch
                        .Id("index")
                        .Add(S["Reindex"], "1", reindex => reindex
                            .Permission(Permissions.ManageAzureSearchIndex)
                            .Action("TriggerReindex", "Admin", new { area = typeof(Startup)!.Namespace }))
                    );
            }

            return Task.CompletedTask;
        }
    }
}
