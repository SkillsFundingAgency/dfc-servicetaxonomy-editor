using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts
{
    public class CloneContext : GraphSyncContext, ICloneItemSyncContext
    {
        public CloneContext(
            ContentItem contentItem,
            ISyncNameProvider syncNameProvider,
            IContentManager contentManager,
            IContentItemVersion contentItemVersion,
            //IServiceProvider serviceProvider,    //todo: use activate utilities
            ILogger<CloneContext> logger,
            ICloneContext? parentContext = null)
        : base (
                contentItem,
                syncNameProvider,
                contentManager,
                contentItemVersion,
                //contentItemVersionFactory.Get(graphReplicaSet.Name),
                parentContext,
                //serviceProvider.GetRequiredService<ILogger<GraphDeleteContext>>())
                logger)
        {
        }
    }
}
