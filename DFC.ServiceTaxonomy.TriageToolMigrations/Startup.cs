using DFC.ServiceTaxonomy.TriageToolMigrations.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data.Migration;
using StartupBase = OrchardCore.Modules.StartupBase;

namespace DFC.ServiceTaxonomy.TriageToolMigrations
{
    public class Startup : StartupBase
    {
        private IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDataMigration, TriageToolMigration>();
        }
    }

}
