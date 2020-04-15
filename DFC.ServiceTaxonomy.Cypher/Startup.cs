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
            services.AddTransient<Models.CypherQuery>();
            services.AddScoped<IQuerySource, CypherQuerySource>();
            services.AddScoped<IDisplayDriver<Query>, Drivers.CypherQueryDisplayDriver>();
            services.AddScoped<INavigationProvider, Menus.AdminMenu>();
            services.AddScoped<IPermissionProvider, Permissions.Permissions>();
        }
    }
}
