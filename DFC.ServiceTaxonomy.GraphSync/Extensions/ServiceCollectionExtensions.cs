using System;
using DFC.ServiceTaxonomy.GraphSync.Configuration;
using DFC.ServiceTaxonomy.GraphSync.Services;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        //todo: why is this called multiple times?
        public static void AddEventGridSubscriptionsApi(this IServiceCollection services, IConfiguration configuration)
        {
            var eventGridSection = configuration.GetSection("EventGridSubscriptions");
            services.Configure<EventGridSubscriptionsConfiguration>(eventGridSection);

            //todo: check config for null and throw meaningful exceptions

            var eventGridSubscriptionsConfig = eventGridSection.Get<EventGridSubscriptionsConfiguration>();

            if (string.IsNullOrEmpty(eventGridSubscriptionsConfig.SubscriptionsApiUrl))
                return;

            services.AddHttpClient(GraphConsumerCommander.EventGridSubscriptionsHttpClientName, client =>
            {
                client.BaseAddress = new Uri(eventGridSubscriptionsConfig.SubscriptionsApiUrl!);
            });
        }
    }
}
