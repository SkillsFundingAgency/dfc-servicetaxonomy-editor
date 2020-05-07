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
            TemplateContext.GlobalMemberAccessStrategy.Register<TabField>();
            TemplateContext.GlobalMemberAccessStrategy.Register<EmptyViewModel>();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddContentField<AccordionField>()
                .UseDisplayDriver<AccordionFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, AccordionFieldSettingsDriver>();

            services.AddContentField<TabField>()
                .UseDisplayDriver<TabFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, TabFieldSettingsDriver>();
        }
    }
}
