using DFC.ServiceTaxonomy.JobProfiles.Module.Handler;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;

namespace DFC.ServiceTaxonomy.JobProfiles.Module
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<IContentHandler, JobProfileContentHandler>();
        }
    }
}
