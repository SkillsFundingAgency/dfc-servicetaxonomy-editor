using DFC.ServiceTaxonomy.Dysac.Indexes;
using DFC.ServiceTaxonomy.Dysac.Interfaces;
using DFC.ServiceTaxonomy.Dysac.Models;
using DFC.ServiceTaxonomy.Dysac.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.GraphQL.Queries;
using OrchardCore.Data.Migration;
using OrchardCore.Flows.Models;
using OrchardCore.Modules;
using YesSql.Indexes;

namespace DFC.ServiceTaxonomy.Dysac
{
    [RequireFeatures("OrchardCore.Apis.GraphQL")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddContentPart<JobProfileCategoriesPart>();
            services.AddScoped<IDataMigration, Migrations>();
            services.AddSingleton<IIndexProvider, JobProfileCategoriesPartIndexProvider>();

            services.AddSingleton<IContentItemService, ContentItemService>();
            services.AddSingleton<IDbQueryService, DbQueryService>();

        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {

        }
    }
}
