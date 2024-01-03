using DFC.ServiceTaxonomy.PageLocation.Models;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis;
using OrchardCore.ContentManagement.GraphQL.Queries;
using OrchardCore.Modules;

namespace DFC.ServiceTaxonomy.PageLocation.GraphQL
{

    [RequireFeatures("OrchardCore.Apis.GraphQL")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddInputObjectGraphType<PageLocationPart, PageLocationInputObjectType>();
            services.AddTransient<IIndexAliasProvider, PageLocationPartIndexAliasProvider>();
            services.AddObjectGraphType<PageLocationPart, PageLocationPartQueryObjectType>();
        }
    }
}
