using System;
using System.Linq;
using DFC.ServiceTaxonomy.Editor.Activities.Tasks;
//using System.Net.Http;
using DFC.ServiceTaxonomy.Editor.Configuration;
using DFC.ServiceTaxonomy.Editor.Drivers;
using DFC.ServiceTaxonomy.Editor.MethodProviders;
using DFC.ServiceTaxonomy.Editor.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
//using OrchardCore.Modules;
using OrchardCore.Scripting;
using OrchardCore.Workflows.Helpers;

namespace DFC.ServiceTaxonomy.Editor
{
    //[RequireFeatures("OrchardCore.Scripting")]
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

            //services.AddScripting();
            services.AddSingleton<IGlobalMethodProvider, ConfigMethodProvider>();

            //todo: create extension method
            //todo: how would this work for topic per contenttype?

            EventGridConfiguration eventGridConfig = Configuration.GetSection("EventGrid").Get<EventGridConfiguration>();
            EventGridTopicConfiguration eventGridTopicConfig = eventGridConfig.Topics.First();

            //todo: cache RestHttpClient's by contenttype??
            //todo: check config for null and throw meaningful exceptions

            //todo: how best to handle injecting? named? inject httpclient into eventgridcontentclient and wrap with rest in ctor?
            // don't use rest?
//            services.AddHttpClient<IEventGridContentClient, EventGridContentClient>(client =>
            services.AddHttpClient<IRestHttpClient, RestHttpClient>(client =>
                {
                    client.BaseAddress = new Uri(eventGridTopicConfig.TopicEndpoint!);
                    client.DefaultRequestHeaders.Add("aeg-sas-key", eventGridTopicConfig.AegSasKey!);
                })
                // .AddPolicyHandler(GetRetryPolicy())
                // .AddPolicyHandler(GetCircuitBreakerPolicy());
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
