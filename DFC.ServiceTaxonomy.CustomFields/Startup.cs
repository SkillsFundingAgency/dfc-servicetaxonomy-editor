using DFC.ServiceTaxonomy.CustomFields.Drivers;
using DFC.ServiceTaxonomy.CustomFields.Fields;
using DFC.ServiceTaxonomy.CustomFields.Settings;
using DFC.ServiceTaxonomy.CustomFields.ViewModels;
using Fluid;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Modules;

namespace DFC.ServiceTaxonomy.CustomFields
{
    public class Startup : StartupBase
    {
        public Startup()
        {
            TemplateContext.GlobalMemberAccessStrategy.Register<AccordionField>();
            TemplateContext.GlobalMemberAccessStrategy.Register<DisplayAccordionFieldViewModel>();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ContentField, AccordionField>();

            services.AddScoped<IContentFieldDisplayDriver, AccordionFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, AccordionFieldSettingsDriver>();
        }
    }
}
