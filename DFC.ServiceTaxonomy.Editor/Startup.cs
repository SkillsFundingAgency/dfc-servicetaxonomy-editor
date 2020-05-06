using System;
using System.Linq;
using DFC.ServiceTaxonomy.Editor.Activities.Tasks;
using DFC.ServiceTaxonomy.Editor.Configuration;
using DFC.ServiceTaxonomy.Editor.Drivers;
using DFC.ServiceTaxonomy.Editor.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrchardCore.Workflows.Helpers;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;

namespace DFC.ServiceTaxonomy.Editor
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry(options =>
                options.InstrumentationKey = Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"]);

            services.AddOrchardCms();

            //services.AddSingleton<IGlobalMethodProvider, ConfigMethodProvider>();

            //todo: create extension method
            //todo: how would this work for topic per contenttype?

            EventGridConfiguration eventGridConfig = Configuration.GetSection("EventGrid").Get<EventGridConfiguration>();
            EventGridTopicConfiguration eventGridTopicConfig = eventGridConfig.Topics.First();

            //todo: cache RestHttpClient's by contenttype??
            //todo: check config for null and throw meaningful exceptions

            //todo: how best to handle injecting? named? inject httpclient into eventgridcontentclient and wrap with rest in ctor?
            // don't use rest?
//            services.AddHttpClient<IEventGridContentClient, EventGridContentClient>(client =>


            var delay = Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay: TimeSpan.FromSeconds(1), retryCount: 5);

            // policy notes
            // we don't add a circuit-breaker (but we might later)
            // we could add knowledge of CloudError to policy, but we don't as errors resulting in CloudError's are probably our fault (with the event contents), as opposed to transient network/http errors

            var retryPolicy =
                HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(delay, (result, timespan, retryAttempt, context) =>
                {
                    //todo: log retries in context of resthttpclient or eventgridcontentclient
                    // services.GetRequiredService<ILogger<EventGridContentClient>>()
                    //     .LogWarning($"Delaying for {timespan}, then making retry {retryAttempt}.");

                });

            services.AddHttpClient<IRestHttpClient, RestHttpClient>(client =>
                {
                    client.BaseAddress = new Uri(eventGridTopicConfig.TopicEndpoint!);
                    client.DefaultRequestHeaders.Add("aeg-sas-key", eventGridTopicConfig.AegSasKey!);
                })
                .AddPolicyHandler(retryPolicy)
                .SetHandlerLifetime(TimeSpan.FromMinutes(3));

            services.AddTransient<IEventGridContentClient, EventGridContentClient>();

            // workflow activities
            services.AddActivity<PublishToEventGridTask, PublishToEventGridTaskDisplay>();
        }

        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseOrchardCore();
        }
    }
}
