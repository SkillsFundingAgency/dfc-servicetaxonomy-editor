using DFC.ServiceTaxonomy.Migration.Migrations;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;
using Microsoft.Extensions.Configuration;

namespace DFC.ServiceTaxonomy.Migration
{
    public class Startup : StartupBase
    {
        private IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDataMigration, RecipeMigration>();
        }
    }
}
