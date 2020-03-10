using System;
using DFC.ServiceTaxonomy.Cypher.Configuration;
using DFC.ServiceTaxonomy.Cypher.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Queries;
using OrchardCore.Security.Permissions;

namespace DFC.ServiceTaxonomy.Cypher
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>();
            services.AddSingleton(configuration.GetSection("Neo4j").Get<Neo4jConfiguration>());

            services.AddTransient<Models.CypherQuery>();
            services.AddScoped<IQuerySource, CypherQuerySource>();
            services.AddScoped<IDisplayDriver<Query>, Drivers.CypherQueryDisplayDriver>();
            services.AddScoped<INavigationProvider, Menus.AdminMenu>();
            services.AddScoped<IPermissionProvider, Permissions.Permissions>();

            services.AddSingleton<INeo4JHelper, Neo4JHelper>();
        }
    }
}
