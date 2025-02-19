using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace DFC.ServiceTaxonomy.JobProfiles.Exporter
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
                    .Add(S["Export Job Profiles Data"], "98", JPExport =>
                        JPExport
                            .AddClass("icon-class-fa-solid fa-file-export")
                            .AddClass("export")
                            .Id("export")
                            .Add(S["Export"], "1", export => export
                                .Permission(Permissions.ManageJobProfilesExporter)
                                .Action("TriggerExport", "Admin", new { area = typeof(Startup)!.Namespace }))
                    );
            }

            return Task.CompletedTask;
        }
    }
}
