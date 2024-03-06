using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.AzureSearch;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.AzureSearch.Converters;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.AzureSearch.Interfaces;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.GraphQL;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Handlers;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Indexes;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Models;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Models.ServiceBus;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.ServiceBus;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.ServiceBus.Converters;
using DFC.ServiceTaxonomy.JobProfiles.DataTransfer.ServiceBus.Interfaces;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Apis;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.GraphQL.Queries;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;
using OrchardCore.Navigation;

using YesSql.Indexes;

namespace DFC.ServiceTaxonomy.JobProfiles.DataTransfer
{
    [RequireFeatures("OrchardCore.Apis.GraphQL")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddContentPart<JobProfileSimplificationPart>();

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
            services.AddScoped<IMessageConverter<RealStory>, RealStoryMessageConverter>();
            services.AddScoped<IRelatedSkillsConverter, RelatedSkillsConverter>();
            services.AddScoped<IDataEventProcessor, DataEventProcessor>();
            services.AddScoped<IServiceBusMessageProcessor, ServiceBusMessageProcessor>();
            services.AddScoped<IDataMigration, Migrations>();

            services.AddScoped<INavigationProvider, AdminMenu>();
            services.TryAddScoped<IReIndexService, ReIndexService>();

            services.AddInputObjectGraphType<JobProfileSimplificationPart, JobProfileSimplificationInputObjectType>();
            services.AddTransient<IIndexAliasProvider, JobProfileSimplificationPartIndexAliasProvider>();

            // Index Providers
            services.AddSingleton<IIndexProvider, JobProfileIndexProvider>();

        }
    }
}
