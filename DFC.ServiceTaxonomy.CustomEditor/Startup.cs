using DFC.ServiceTaxonomy.CustomEditor.Drivers;
using DFC.ServiceTaxonomy.CustomEditor.Shapes;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Modules;

namespace DFC.ServiceTaxonomy.CustomEditor
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IShapeTableProvider, SummaryAdminShapes>();
            services.AddScoped<IContentDisplayDriver, CustomContentDriver>();
        }
    }
}
