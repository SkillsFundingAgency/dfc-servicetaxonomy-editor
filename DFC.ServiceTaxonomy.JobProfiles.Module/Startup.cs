using DFC.ServiceTaxonomy.JobProfiles.Module.Handler;
using DFC.ServiceTaxonomy.JobProfiles.Service.EFDataModels;
using DFC.ServiceTaxonomy.JobProfiles.Service.Interfaces;
using DFC.ServiceTaxonomy.JobProfiles.Service.Models;
using DFC.ServiceTaxonomy.JobProfiles.Service.Repositories;
using DFC.ServiceTaxonomy.JobProfiles.Service.Services;
using Microsoft.EntityFrameworkCore;
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
            // CMS
            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<IContentHandler, JobProfileContentHandler>();

            // Repositories
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
