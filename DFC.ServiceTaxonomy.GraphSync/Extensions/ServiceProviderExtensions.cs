using System;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace DFC.ServiceTaxonomy.GraphSync.Extensions
{
    public static class ServiceProviderExtensions
    {
        public static ISyncNameProvider GetSyncNameProvider(this IServiceProvider serviceProvider, string contentType)
        {
            ISyncNameProvider syncNameProvider = serviceProvider.GetRequiredService<ISyncNameProvider>();
            syncNameProvider.ContentType = contentType;
            return syncNameProvider;
        }
    }
}
