﻿using System;
using System.Net.Http;
using DFC.ServiceTaxonomy.Events.Configuration;
using DFC.ServiceTaxonomy.Events.Services;
using DFC.ServiceTaxonomy.Events.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds services to enable publishing of content events to Azure Event Grid.
        /// </summary>
        /// <remarks>
        /// Policy notes:
        /// We don't add a circuit-breaker (but we might later).
        /// We could add knowledge of CloudError to policy, but we don't as errors resulting in CloudError's are
        /// probably our fault (with the event contents), as opposed to transient network/http errors.
        /// We might want to change the Handler lifetime from the default of 2 minutes, using
        /// .SetHandlerLifetime(TimeSpan.FromMinutes(3));
        /// it's a balance between keeping sockets open and latency in handling dns updates.
        /// </remarks>
        public static void AddEventGridPublishing(this IServiceCollection services, IConfiguration configuration)
        {
            var eventGridSection = configuration.GetSection("EventGrid");
            services.Configure<EventGridConfiguration>(eventGridSection);

            //todo: check config for null and throw meaningful exceptions

            EventGridConfiguration eventGridConfig = eventGridSection.Get<EventGridConfiguration>();

            // publishing an event should be quick, so set the timeout low
            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(5);

            foreach (EventGridTopicConfiguration eventGridTopicConfig in eventGridConfig.Topics)
            {
                var delay = Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay: TimeSpan.FromSeconds(1), retryCount: 5);

                services.AddHttpClient(eventGridTopicConfig.ContentType, client =>
                    {
                        client.BaseAddress = new Uri(eventGridTopicConfig.TopicEndpoint!);
                        client.DefaultRequestHeaders.Add("aeg-sas-key", eventGridTopicConfig.AegSasKey!);
                    })
                    .AddPolicyHandler((services, request) => HttpPolicyExtensions
                        .HandleTransientHttpError()
                        .WaitAndRetryAsync(delay, (result, timespan, retryAttempt, context) =>
                        {
                            services.GetService<ILogger<EventGridContentClient>>()
                                .LogWarning($"Delaying for {timespan}, then making retry {retryAttempt}.");
                        }))
                    .AddPolicyHandler(timeoutPolicy);

                services.AddTransient<IEventGridContentClient, EventGridContentClient>();
            }

            services.AddTransient<IEventGridContentRestHttpClientFactory, EventGridContentRestHttpClientFactory>();

            //todo: for reuse in a nuget, will need these and all that GraphSyncHelper brings in
            // probably need to split GraphSyncHelper, so it doesn't need all the current dependencies
            // services.AddScoped<IContentHandler, PublishToEventGridHandler>();
            //
            // services.AddTransient<IGraphSyncHelper, GraphSyncHelper>();
            // services.AddTransient<IGraphSyncHelperCSharpScriptGlobals, GraphSyncHelperCSharpScriptGlobals>();
            // services.AddTransient<IContentHelper, ContentHelper>();
            // services.AddTransient<IServiceTaxonomyHelper, ServiceTaxonomyHelper>();
            //
            // services.AddSingleton<IPublishedContentItemVersion>(new PublishedContentItemVersion(configuration));
            // services.AddSingleton<IPreviewContentItemVersion>(new PreviewContentItemVersion(configuration));
            // services.AddSingleton<INeutralContentItemVersion>(new NeutralContentItemVersion());
        }
    }
}
