using DFC.ServiceTaxonomy.DataAccess.Repositories;
using DFC.ServiceTaxonomy.JobProfiles.Module.Handlers;
using DFC.ServiceTaxonomy.JobProfiles.Module.EFDataModels;
using DFC.ServiceTaxonomy.JobProfiles.Module.Interfaces;
using DFC.ServiceTaxonomy.JobProfiles.Module.Models;
using DFC.ServiceTaxonomy.JobProfiles.Module.Repositories;
using DFC.ServiceTaxonomy.JobProfiles.Module.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Modules;


namespace DFC.ServiceTaxonomy.JobProfiles.Module
{
    public class Startup : StartupBase
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration) =>
            _configuration = configuration;

        public override void ConfigureServices(IServiceCollection services)
        {
            // CMS
            services.AddScoped<IContentHandler, SocCodeContentHandler>();
            services.AddScoped<IContentHandler, JobProfileContentHandler>();

            // Repositories
            services.AddDbContext<DfcDevOnetSkillsFrameworkContext>(options => options.UseSqlServer(_configuration.GetConnectionString("SkillsFrameworkDB")));
            services.AddScoped<ISocMappingRepository, SocMappingRepository>();
            services.AddScoped<ISkillsRepository, SkillsOueryRepository>();
            services.AddScoped<IQueryRepository<FrameworkSkillSuppression>, SuppressionsQueryRepository>();
            services.AddScoped<IQueryRepository<FrameworkContent>, ContentReferenceQueryRepository>();
            services.AddScoped<IQueryRepository<FrameworkSkillCombination>, CombinationsQueryRepository>();
            services.AddTransient(typeof(IGenericIndexRepository<>), typeof(GenericIndexRepository<>));

            // Services
            services.AddScoped<ISkillFrameworkBusinessRuleEngine, SkillFrameworkBusinessRuleEngine>();
            services.AddScoped<ISkillsFrameworkService, SkillsFrameworkService>();

        }
    }
}
