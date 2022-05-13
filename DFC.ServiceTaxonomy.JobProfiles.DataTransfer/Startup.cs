﻿using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.AzureSearch;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.AzureSearch.Converters;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.AzureSearch.Interfaces;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Handlers;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Indexes;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Models.ServiceBus;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.ServiceBus;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.ServiceBus.Converters;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.ServiceBus.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Data.Migration;
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
            services.AddScoped<IMessageConverter<JobProfileMessage>, JobProfileMessageConverter>();
            services.AddScoped<IMessageConverter<HowToBecomeData>, HowToBecomeMessageConverter>();
            services.AddScoped<IMessageConverter<WhatYouWillDoData>, WhatYouWillDoMessageConverter>();
            services.AddScoped<IMessageConverter<WhatItTakesData>, WhatItTakesMessageConverter>();
            services.AddScoped<IMessageConverter<SocCodeItem>, SocCodeMessageConverter>();
            services.AddScoped<IRelatedSkillsConverter, RelatedSkillsConverter>();
            services.AddScoped<IDataEventProcessor, DataEventProcessor>();
            services.AddScoped<IServiceBusMessageProcessor, ServiceBusMessageProcessor>();
            services.AddScoped<IDataMigration, Migrations>();

            // Index Providers
            services.AddSingleton<IIndexProvider, JobProfileIndexProvider>();

        }
    }
}