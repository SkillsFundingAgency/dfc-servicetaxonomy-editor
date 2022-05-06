using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.AzureSearch;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.AzureSearch.Converters;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.AzureSearch.Interfaces;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Handlers;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Indexes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Modules;
using YesSql.Indexes;

namespace DFC.ServiceTaxonomy.JobProfiles.DataTransfer
{
    public class Startup : StartupBase
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration) =>
            _configuration = configuration;

        public override void ConfigureServices(IServiceCollection services)
        {
            // CMS
            services.AddScoped<IContentHandler, ServiceBusContentHandler>();
            services.AddScoped<IContentHandler, JobProfileAzureSearchIndexHandler>();
            services.AddScoped<IMessageConverter<Models.AzureSearch.JobProfileIndex>, JobProfileIndexMessageConverter>();
            services.AddScoped<IAzureSearchDataProcessor, AzureSearchDataProcessor>();


            // Index Providers
            services.AddSingleton<IIndexProvider, JobProfileIndexProvider>();

        }
    }
}
