using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphVisualiser.Controllers;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace DFC.ServiceTaxonomy.GraphVisualiser.Services
{
    public class AdminMenuService : INavigationProvider
    {
        private readonly IStringLocalizer S;

        public AdminMenuService(IStringLocalizer<AdminMenuService> localizer)
        {
            S = localizer;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder
                .Add(S["Visualiser"], "after", rootView => rootView
                   .Add(S["Ontology"], "1", ontology => ontology
                       .Action(nameof(VisualiseController.Viewer), "Visualise", new { area = typeof(Startup)!.Namespace })));

            return Task.CompletedTask;
        }
    }
}
