using DFC.ServiceTaxonomy.Slack.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DFC.ServiceTaxonomy.Slack
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSlackMessagePublishing(this IServiceCollection services, IConfiguration configuration, bool includeGlobalExceptionLogging = false)
        {
            services.Configure<SlackMessagePublishingConfiguration>(configuration.GetSection("SlackMessagePublishingConfiguration"));

            if (includeGlobalExceptionLogging)
            {
                services.Configure<MvcOptions>(
                    options => options.Filters.Add(typeof(UnhandledExceptionFilterAttribute)));
            }

            return services.AddTransient<ISlackMessagePublisher, SlackMessagePublisher>();
        }
    }
}
