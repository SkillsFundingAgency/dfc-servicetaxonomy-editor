﻿using DFC.ServiceTaxonomy.JobProfiles.Module.Handlers;
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
using OrchardCore.Modules;

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
            //services.AddScoped<IContentHandler, JobProfileContentHandler>();
            services.AddScoped<IMessageConverter<JobProfileMessage>, JobProfileMessageConverter>();
            services.AddScoped<IMessageConverter<HowToBecomeData>, HowToBecomeMessageConverter>();
            services.AddScoped<IMessageConverter<WhatYouWillDoData>, WhatYouWillDoMessageConverter>();
            services.AddScoped<IMessageConverter<WhatItTakesData>, WhatItTakesMessageConverter>();
            services.AddScoped<IMessageConverter<SocCodeItem>, SocCodeMessageConverter>();
            services.AddScoped<IDataEventProcessor, DataEventProcessor>();
            services.AddScoped<IContentHandler, ServiceBusContentHandler>();
            services.AddScoped<IServiceBusMessageProcessor, ServiceBusMessageProcessor>();

            // Repositories
            services.AddDbContext<DfcDevOnetSkillsFrameworkContext>(options => options.UseSqlServer(configuration.GetConnectionString("SkillsFramworkDB")));
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
