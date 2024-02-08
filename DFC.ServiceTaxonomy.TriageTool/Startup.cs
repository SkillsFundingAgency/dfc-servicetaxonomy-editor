using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.PageLocation.GraphQL;
using DFC.ServiceTaxonomy.PageLocation.Indexes;
using DFC.ServiceTaxonomy.PageLocation.Models;
using DFC.ServiceTaxonomy.TriageTool.GraphQL;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.GraphQL;
using OrchardCore.ContentManagement.GraphQL.Queries;
using OrchardCore.Data.Migration;
using OrchardCore.Html.Models;
using OrchardCore.Modules;
using OrchardCore.Sitemaps.Models;
using YesSql.Indexes;

namespace DFC.ServiceTaxonomy.TriageTool
{
    [RequireFeatures("OrchardCore.Apis.GraphQL")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddContentPart<PagePart>();
            services.AddInputObjectGraphType<PagePart, PageInputObjectType>();
            services.AddObjectGraphType<PagePart, PageQueryObjectType>();
            services.AddGraphQLFilterType<ContentItem,PagePartGraphQLFilter>();
          
            //services.AddInputObjectGraphType<PagePart, PagePartWhereInputObjectGraphType>();
            services.AddScoped<IDataMigration, Migrations>();
            services.AddSingleton<IIndexProvider, PageItemsPartIndexProvider>();
           
            services.AddTransient<IIndexAliasProvider, PagePartIndexAliasProvider>();
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {

        }
    }
}
