﻿using System;
using System.Net.Http;
using DFC.ServiceTaxonomy.Events.Activities.Tasks;
using DFC.ServiceTaxonomy.Events.Configuration;
using DFC.ServiceTaxonomy.Events.Drivers;
using DFC.ServiceTaxonomy.Events.Services;
using DFC.ServiceTaxonomy.Events.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Workflows.Helpers;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;

namespace DFC.ServiceTaxonomy.Events.Extensions
{
    //16.5.1
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
        /// </remarks>
        public static void AddEventGridPublishing(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IEventGridContentRestHttpClientFactory, EventGridContentRestHttpClientFactory>();

            //todo: check config for null and throw meaningful exceptions

            EventGridConfiguration eventGridConfig = configuration.GetSection("EventGrid").Get<EventGridConfiguration>();

            var delay = Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay: TimeSpan.FromSeconds(1), retryCount: 5);

            // publishing an event should be quick, so set the timeout low
            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(5);

            foreach (EventGridTopicConfiguration eventGridTopicConfig in eventGridConfig.Topics)
            {
                //todo: check config for null and throw meaningful exceptions

                // we could add knowledge of CloudError to policy, but we don't as errors resulting in CloudError's are probably our fault (with the event contents), as opposed to transient network/http errors

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
                    // leave as default 2 minutes for now
                    //.SetHandlerLifetime(TimeSpan.FromMinutes(3));

                services.AddTransient<IEventGridContentClient, EventGridContentClient>();
            }

            // workflow activities
            services.AddActivity<PublishToEventGridTask, PublishToEventGridTaskDisplay>();
        }
    }
}
