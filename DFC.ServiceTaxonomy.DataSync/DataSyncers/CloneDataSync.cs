using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Contexts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Items;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers
{
    public class CloneDataSync : ICloneDataSync
    {
        private readonly IEnumerable<IContentItemDataSyncer> _itemSyncers;
        private readonly ISyncNameProvider _syncNameProvider;
        private readonly IPreviewContentItemVersion _previewContentItemVersion;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CloneDataSync> _logger;

        public CloneDataSync(
            IEnumerable<IContentItemDataSyncer> itemSyncers,
            ISyncNameProvider syncNameProvider,
            IPreviewContentItemVersion previewContentItemVersion,    //todo: ??
            IServiceProvider serviceProvider,
            ILogger<CloneDataSync> logger)
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
            var context = new CloneContext(contentItem, this, _syncNameProvider, contentManager,
                _previewContentItemVersion, _serviceProvider, parentContext);

            foreach (IContentItemDataSyncer itemSyncer in _itemSyncers)
            {
                //todo: allow syncers to chain or not? probably not
                //todo: code shared with MergeDataSyncer. might want to introduce common base if sharing continues
                if (itemSyncer.CanSync(context.ContentItem))
                {
                    await itemSyncer.MutateOnClone(context);
                }
            }
        }
    }
}
