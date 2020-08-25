using System;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts
{
    public class CloneContext : GraphSyncContext, ICloneItemSyncContext
    {
        public ICloneGraphSync CloneGraphSync { get; }

        public CloneContext(
            ContentItem contentItem,
            ICloneGraphSync cloneGraphSync,
            ISyncNameProvider syncNameProvider,
            IContentManager contentManager,
            IContentItemVersion contentItemVersion,
            // ActivatorUtilities.CreateInstance can't handle nulls, even with multiple ctors
            //ILogger<CloneContext> logger,
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
            CloneGraphSync = cloneGraphSync;
        }

    }
}
