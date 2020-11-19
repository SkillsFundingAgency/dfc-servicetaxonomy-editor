using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DFC.ServiceTaxonomy.Slack
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSlackMessagePublishing(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SlackMessagePublishingConfiguration>(configuration.GetSection("SlackMessagePublishingConfiguration"));
            return services.AddTransient<ISlackMessagePublisher, SlackMessagePublisher>();
        }
    }
}
