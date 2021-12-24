//using DFC.ServiceTaxonomy.JobProfiles.Module.Handlers;
//using DFC.ServiceTaxonomy.JobProfiles.Module.Handlers;
using DFC.ServiceTaxonomy.JobProfiles.Module.Handlers;
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
using OrchardCore.ContentManagement.Handlers;
//using OrchardCore.ContentManagement.Handlers;
//using OrchardCore.ContentManagement.Handlers;
//using OrchardCore.Data.Migration;
using OrchardCore.Modules;

namespace DFC.ServiceTaxonomy.JobProfiles.Module
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // CMS
            //services.AddScoped<IDataMigration, Migrations>();
            //services.AddScoped<IContentHandler, SocCodeContentHandler>();
            //services.AddScoped<IContentHandler, JobProfileContentHandler>();
            services.AddScoped<IMessageConverter<JobProfileMessage>, JobProfileMessageConverter>();
            services.AddScoped<IMessageConverter<HowToBecomeData>, HowToBecomeMessageConverter>();
            services.AddScoped<IDataEventProcessor, DataEventProcessor>();
            services.AddScoped<IContentHandler, ServiceBusContentHandler>();
            services.AddScoped<IServiceBusMessageProcessor, ServiceBusMessageProcessor>();

            // Repositories

            // TODO: SQL Server connection string should be in settings
            services.AddDbContext<DfcDevOnetSkillsFrameworkContext>(options => options.UseSqlServer("Server=xxx;Database=dfc-dev-onetskillsframework;Trusted_Connection=True;"));
            services.AddScoped<ISocMappingRepository, SocMappingRepository>();
            services.AddScoped<ISkillsRepository, SkillsOueryRepository>();
            services.AddScoped<IQueryRepository<FrameworkSkillSuppression>, SuppressionsQueryRepository>();
            services.AddScoped<IQueryRepository<FrameworkContent>, ContentReferenceQueryRepository>();
            services.AddScoped<IQueryRepository<FrameworkSkillCombination>, CombinationsQueryRepository>();

            // Services
            services.AddScoped<ISkillFrameworkBusinessRuleEngine, SkillFrameworkBusinessRuleEngine>();
            services.AddScoped<ISkillsFrameworkService, SkillsFrameworkService>();
        }
    }
}
