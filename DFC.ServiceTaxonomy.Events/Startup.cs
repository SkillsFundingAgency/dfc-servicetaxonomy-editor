using System;
using System.Net.Http;
using DFC.ServiceTaxonomy.Events.Activities.Tasks;
using DFC.ServiceTaxonomy.Events.Configuration;
using DFC.ServiceTaxonomy.Events.Drivers;
using DFC.ServiceTaxonomy.Events.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.Workflows.Helpers;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;

namespace DFC.ServiceTaxonomy.Events
{
    public class Startup : StartupBase
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public override void ConfigureServices(IServiceCollection services)
        {
            //services.AddSingleton<IGlobalMethodProvider, ConfigMethodProvider>();

            //todo: create extension method
            //todo: how would this work for topic per contenttype?

            services.AddTransient<IEventGridContentRestHttpClientFactory, EventGridContentRestHttpClientFactory>();

            //todo: check config for null and throw meaningful exceptions

            EventGridConfiguration eventGridConfig = Configuration.GetSection("EventGrid").Get<EventGridConfiguration>();

            var delay = Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay: TimeSpan.FromSeconds(1), retryCount: 5);

            var retryPolicy =
                HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .WaitAndRetryAsync(delay, (result, timespan, retryAttempt, context) =>
                    {
                        //todo: log retries in context of resthttpclient or eventgridcontentclient
                        // services.GetRequiredService<ILogger<EventGridContentClient>>()
                        //     .LogWarning($"Delaying for {timespan}, then making retry {retryAttempt}.");
                    });

            // publishing an event should be quick, so set the timeout low
            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(5);

            foreach (EventGridTopicConfiguration eventGridTopicConfig in eventGridConfig.Topics)
            {
                //todo: cache RestHttpClient's by contenttype??
                //todo: check config for null and throw meaningful exceptions

                //todo: how best to handle injecting? named? inject httpclient into eventgridcontentclient and wrap with rest in ctor?
                // don't use rest?
//            services.AddHttpClient<IEventGridContentClient, EventGridContentClient>(client =>

                // policy notes
                // we don't add a circuit-breaker (but we might later)
                // we could add knowledge of CloudError to policy, but we don't as errors resulting in CloudError's are probably our fault (with the event contents), as opposed to transient network/http errors

//                services.AddHttpClient<IRestHttpClient, RestHttpClient>(client =>
                services.AddHttpClient(eventGridTopicConfig.ContentType, client =>
                    {
                        client.BaseAddress = new Uri(eventGridTopicConfig.TopicEndpoint!);
                        client.DefaultRequestHeaders.Add("aeg-sas-key", eventGridTopicConfig.AegSasKey!);
                    })
                    .AddPolicyHandler(retryPolicy)
                    .AddPolicyHandler(timeoutPolicy);
                    // leave as default 2 minutes for now
                    //.SetHandlerLifetime(TimeSpan.FromMinutes(3));

                services.AddTransient<IEventGridContentClient, EventGridContentClient>();
            }

            // workflow activities
            services.AddActivity<PublishToEventGridTask, PublishToEventGridTaskDisplay>();
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            //
        }
    }
}
