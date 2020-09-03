using DFC.ServiceTaxonomy.Content.Services;
using DFC.ServiceTaxonomy.Content.Services.Interface;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

namespace DFC.ServiceTaxonomy.Content.Extensions
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IContentItemsService, ContentItemsService>();
        }
    }
}
