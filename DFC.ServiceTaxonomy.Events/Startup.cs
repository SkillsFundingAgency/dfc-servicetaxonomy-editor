using DFC.ServiceTaxonomy.Events.Handlers;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Modules;

namespace DFC.ServiceTaxonomy.Events
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IContentHandler, PublishToEventGridHandler>();
        }
    }
}
