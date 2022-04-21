using System;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace DFC.ServiceTaxonomy.DataSync.Extensions
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
