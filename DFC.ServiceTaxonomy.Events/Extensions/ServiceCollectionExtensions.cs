using System;
using System.Net.Http;
using DFC.ServiceTaxonomy.Events.Configuration;
using DFC.ServiceTaxonomy.Events.Handlers;
using DFC.ServiceTaxonomy.Events.Services;
using DFC.ServiceTaxonomy.Events.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement.Handlers;
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

            services.AddScoped<IContentHandler, PublishToEventGridHandler>();
        }
    }
}
