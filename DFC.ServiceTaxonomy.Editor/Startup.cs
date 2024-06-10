using System;
using System.Collections.Generic;
using System.Net.Http;
using DFC.Common.SharedContent.Pkg.Netcore;
using DFC.Common.SharedContent.Pkg.Netcore.Infrastructure;
using DFC.Common.SharedContent.Pkg.Netcore.Infrastructure.Strategy;
using DFC.Common.SharedContent.Pkg.Netcore.Interfaces;
using DFC.Common.SharedContent.Pkg.Netcore.Model.Response;
using DFC.Common.SharedContent.Pkg.Netcore.RequestHandler;
using DFC.ServiceTaxonomy.Content.Configuration;
using DFC.ServiceTaxonomy.CustomEditor.Configuration;
using DFC.ServiceTaxonomy.Editor.Security;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrchardCore.Media;

namespace DFC.ServiceTaxonomy.Editor
{
    public class Startup
    {
        private const string RedisCacheConnectionStringAppSettings = "Cms:RedisCacheConnectionString";
        public const string ConfigAppSettings = "Configuration";
        private const string StaxGraphApiUrlAppSettings = "Cms:GraphApiUrl";

        public Startup(IConfiguration configuration) =>
            Configuration = configuration;

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry(options =>
                options.InstrumentationKey = Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"]);

            services.AddDistributedMemoryCache();
            services.AddSession();

            services.AddOrchardCms()
                .ConfigureServices(se => se.ConfigureHtmlSanitizer((sanitizer) =>
                {
                    sanitizer.AllowDataAttributes = true;
                    sanitizer.AllowedAttributes.Add("id");
                    sanitizer.AllowedAttributes.Add("aria-labelledby");
                    sanitizer.AllowedTags.Add("iframe");
                    sanitizer.AllowedTags.Add("svg");
                    sanitizer.AllowedTags.Add("path");
                    sanitizer.AllowedTags.Add("form");
                    sanitizer.AllowedAttributes.Add("fill");
                    sanitizer.AllowedAttributes.Add("d");
                    sanitizer.AllowedAttributes.Add("xmlns");
                    sanitizer.AllowedAttributes.Add("viewBox");
                    sanitizer.AllowedAttributes.Add("allowfullscreen");
                    sanitizer.AllowedSchemes.Add("mailto");
                    sanitizer.AllowedSchemes.Add("tel");

                    sanitizer.AllowedAttributes.Remove("style");
                }));

            //todo: do this in each library??? if so, make sure it doesn't add services or config twice

            services.Configure<CookiePolicyOptions>(options => options.Secure = CookieSecurePolicy.Always);
            services.AddEventGridPublishing(Configuration);

            services.AddOrchardCore()
                .ConfigureServices(s =>
                {
                    s.ConfigureApplicationCookie(options => options.Cookie.Name = "stax_Default");
                    s.AddAntiforgery(options => options.Cookie.Name = "staxantiforgery_Default");
                }, order: 10);

            services.Configure<PagesConfiguration>(Configuration.GetSection("Pages"));
            services.Configure<JobProfilesConfiguration>(Configuration.GetSection("JobProfiles"));
            services.Configure<AzureAdSettings>(Configuration.GetSection("AzureAdSettings"));

            services.AddSingleton<IGraphQLClient>(s =>
            {
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 
                var option = new GraphQLHttpClientOptions()
                {
                    EndPoint = new Uri(Configuration.GetSection(StaxGraphApiUrlAppSettings).Get<string>() ?? throw new ArgumentNullException()),

                    HttpMessageHandler = new CmsRequestHandler(
                        s.GetService<IHttpClientFactory>(),
                        s.GetService<IConfiguration>(),
                        s.GetService<IHttpContextAccessor>() ?? throw new ArgumentNullException(),
                        s.GetService<IMemoryCache>()),
                };
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
#pragma warning restore CS8604 // Possible null reference argument.
                var client = new GraphQLHttpClient(option, new NewtonsoftJsonSerializer());
                return client;
            });

            //Add any additional strategies that need to be refreshed here
            services.AddSingleton<ISharedContentRedisInterfaceStrategyWithRedisExpiry<JobProfileCurrentOpportunitiesResponse>, JobProfileCurrentOpportunitiesStrategy>();
            services.AddSingleton<ISharedContentRedisInterfaceStrategyWithRedisExpiry<JobProfileSkillsResponse>, JobProfileSkillsStrategy>();
            services.AddSingleton<ISharedContentRedisInterfaceStrategyWithRedisExpiry<JobProfileCareerPathAndProgressionResponse>, JobProfileCareerPathAndProgressionStrategy>();
            services.AddSingleton<ISharedContentRedisInterfaceStrategyWithRedisExpiry<RelatedCareersResponse>, JobProfileRelatedCareersQueryStrategy>();
            services.AddSingleton<ISharedContentRedisInterfaceStrategyWithRedisExpiry<JobProfileHowToBecomeResponse>, JobProfileHowToBecomeQueryStrategy>();
            services.AddSingleton<ISharedContentRedisInterfaceStrategyWithRedisExpiry<JobProfilesOverviewResponse>, JobProfileOverviewProfileSpecificQueryStrategy>();
            services.AddSingleton<ISharedContentRedisInterfaceStrategyWithRedisExpiry<JobProfileWhatYoullDoResponse>, JobProfileWhatYoullDoQueryStrategy>();
            services.AddSingleton<ISharedContentRedisInterfaceStrategyWithRedisExpiry<JobProfileVideoResponse>, JobProfileVideoQueryStrategy>();
            services.AddSingleton<ISharedContentRedisInterfaceStrategyWithRedisExpiry<JobProfileCurrentOpportunitiesGetbyUrlReponse>, JobProfileCurrentOpportunitiesGetByUrlStrategy>();

            services.AddStackExchangeRedisCache(options => { options.Configuration = Configuration.GetSection(RedisCacheConnectionStringAppSettings).Get<string>(); });
            services.AddSingleton<ISharedContentRedisInterfaceStrategyFactory, SharedContentRedisStrategyFactory>();
            services.AddSingleton<ISharedContentRedisInterface, SharedContentRedis>();

            services.PostConfigure(SetupMediaConfig());
        }

        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSession();
            app.UseCookiePolicy();
            // UseSecurityHeaders must come before UseOrchardCore
            app.UsePoweredByOrchardCore(false);
            app.UseSecurityHeaders(Configuration)
                .UseOrchardCore();
        }

        private Action<MediaOptions> SetupMediaConfig() =>
            o =>
            {
                o.AllowedFileExtensions = new HashSet<string>
                {
                    ".jpg",
                    ".png",
                    ".gif",
                    ".ico",
                    ".svg"
                };
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                o.CdnBaseUrl = Configuration.GetValue<string>(Constants.Common.DigitalAssetsCdnKey).TrimEnd('/');
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            };
    }
}
