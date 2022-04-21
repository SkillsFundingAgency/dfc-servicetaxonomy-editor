using System;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Contexts
{
    public class CloneContext : DataSyncContext, ICloneItemSyncContext
    {
        public ICloneDataSync CloneDataSync { get; }

        public CloneContext(
            ContentItem contentItem,
            ICloneDataSync cloneDataSync,
            ISyncNameProvider syncNameProvider,
            IContentManager contentManager,
            IContentItemVersion contentItemVersion,
            IServiceProvider serviceProvider,
            ICloneContext? parentContext = null)
            : base (
                contentItem,
                syncNameProvider,
                contentManager,
                contentItemVersion,
                parentContext,
                serviceProvider.GetRequiredService<ILogger<CloneContext>>())
        {
            CloneDataSync = cloneDataSync;
        }

    }
}
