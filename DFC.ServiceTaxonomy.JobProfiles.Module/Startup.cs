using DFC.ServiceTaxonomy.JobProfiles.Module.Handlers;
using DFC.ServiceTaxonomy.JobProfiles.Module.Indexes;
using DFC.ServiceTaxonomy.JobProfiles.Module.Models.ServiceBus;
using DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling;
using DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling.Converters;
using DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling.Interfaces;
using DFC.ServiceTaxonomy.JobProfiles.Service.EFDataModels;
using DFC.ServiceTaxonomy.JobProfiles.Service.Interfaces;
using DFC.ServiceTaxonomy.JobProfiles.Service.Models;
using DFC.ServiceTaxonomy.JobProfiles.Service.Repositories;
using DFC.ServiceTaxonomy.JobProfiles.Service.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;
using YesSql.Indexes;
using DFC.ServiceTaxonomy.DataAccess.Repositories;
using DFC.ServiceTaxonomy.JobProfiles.Module.AzureSearchIndexHandling.Interfaces;
using DFC.ServiceTaxonomy.JobProfiles.Module.AzureSearchIndexHandling;
using DFC.ServiceTaxonomy.JobProfiles.Module.AzureSearchIndexHandling.Converters;

namespace DFC.ServiceTaxonomy.JobProfiles.Module
{
    public class Startup : StartupBase
    {
        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            // CMS
            services.AddScoped<IContentHandler, SocCodeContentHandler>();
            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<IContentHandler, JobProfileContentHandler>();
            services.AddScoped<IMessageConverter<JobProfileMessage>, JobProfileMessageConverter>();
            services.AddScoped<IMessageConverter<HowToBecomeData>, HowToBecomeMessageConverter>();
            services.AddScoped<IMessageConverter<WhatYouWillDoData>, WhatYouWillDoMessageConverter>();
            services.AddScoped<IMessageConverter<WhatItTakesData>, WhatItTakesMessageConverter>();
            services.AddScoped<IMessageConverter<SocCodeItem>, SocCodeMessageConverter>();
            services.AddScoped<IMessageConverter<Models.AzureSearch.JobProfileIndex>, JobProfileIndexMessageConverter>();
            services.AddScoped<IRelatedSkillsConverter, RelatedSkillsConverter>();
            services.AddScoped<IDataEventProcessor, DataEventProcessor>();
            services.AddScoped<IContentHandler, ServiceBusContentHandler>();
            services.AddScoped<IAzureSearchDataProcessor, AzureSearchDataProcessor>();
            services.AddScoped<IServiceBusMessageProcessor, ServiceBusMessageProcessor>();
            services.AddScoped<IContentHandler, JobProfileAzureSearchIndexHandler>();

            // Repositories
            services.AddDbContext<DfcDevOnetSkillsFrameworkContext>(options => options.UseSqlServer(configuration.GetConnectionString("SkillsFrameworkDB")));
            services.AddScoped<ISocMappingRepository, SocMappingRepository>();
            services.AddScoped<ISkillsRepository, SkillsOueryRepository>();
            services.AddScoped<IQueryRepository<FrameworkSkillSuppression>, SuppressionsQueryRepository>();
            services.AddScoped<IQueryRepository<FrameworkContent>, ContentReferenceQueryRepository>();
            services.AddScoped<IQueryRepository<FrameworkSkillCombination>, CombinationsQueryRepository>();
            services.AddTransient(typeof(IGenericIndexRepository<>), typeof(GenericIndexRepository<>));

            // Services
            services.AddScoped<ISkillFrameworkBusinessRuleEngine, SkillFrameworkBusinessRuleEngine>();
            services.AddScoped<ISkillsFrameworkService, SkillsFrameworkService>();

            // Index Providers
            services.AddSingleton<IIndexProvider, JobProfileIndexProvider>();
        }
    }
}
