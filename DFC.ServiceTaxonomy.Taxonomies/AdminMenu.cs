using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Taxonomies.Settings;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace DFC.ServiceTaxonomy.Taxonomies
{
    public class AdminMenu : AdminNavigationProvider
    {
        private readonly IStringLocalizer S;

        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            S = localizer;
        }

        protected override ValueTask BuildAsync(NavigationBuilder builder)
        {
            builder.Add(S["Configuration"], configuration => configuration
                       .Add(S["Settings"], "1", settings => settings
                            .Add(S["Taxonomy Filters"], S["Taxonomy Filters"].PrefixPosition(), admt => admt
                            .AddClass("taxonomyfilters").Id("taxonomyfilters")
                                .Permission(Permissions.ManageTaxonomies)
                                .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = TaxonomyContentsAdminListSettingsDisplayDriver.GroupId })
                                .LocalNav()
                    )));

            return ValueTask.CompletedTask;
        }
    }
}
