using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Items;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    public class CloneGraphSync : ICloneGraphSync
    {
        private readonly IEnumerable<IContentItemGraphSyncer> _itemSyncers;
        private readonly ISyncNameProvider _syncNameProvider;
        private readonly IPreviewContentItemVersion _previewContentItemVersion;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CloneGraphSync> _logger;

        public CloneGraphSync(
            IEnumerable<IContentItemGraphSyncer> itemSyncers,
            ISyncNameProvider syncNameProvider,
            IPreviewContentItemVersion previewContentItemVersion,    //todo: ??
            IServiceProvider serviceProvider,
            ILogger<CloneGraphSync> logger)
        {
            _itemSyncers = itemSyncers.OrderByDescending(s => s.Priority);
            _syncNameProvider = syncNameProvider;
            _previewContentItemVersion = previewContentItemVersion;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task MutateOnClone(
            ContentItem contentItem,
            IContentManager contentManager,
            ICloneContext? parentContext = null)
        {
            // var context = ActivatorUtilities.CreateInstance<CloneContext>(_serviceProvider,
            //     contentItem, _syncNameProvider, contentManager, _previewContentItemVersion,
            //     parentContext);

            var context = new CloneContext(contentItem, this, _syncNameProvider, contentManager,
                _previewContentItemVersion, _serviceProvider, parentContext);

            foreach (IContentItemGraphSyncer itemSyncer in _itemSyncers)
            {
                //todo: allow syncers to chain or not? probably not
                //todo: code shared with MergeGraphSyncer. might want to introduce common base if sharing continues
                if (itemSyncer.CanSync(context.ContentItem))
                {
                    await itemSyncer.MutateOnClone(context);
                }
            }
        }
    }
}
