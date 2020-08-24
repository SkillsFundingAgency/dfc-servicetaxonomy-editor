using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Items;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    public class CloneGraphSync : ICloneGraphSync
    {
        private readonly IEnumerable<IContentItemGraphSyncer> _itemSyncers;
        private readonly Logger<CloneGraphSync> _logger;

        public CloneGraphSync(
            IEnumerable<IContentItemGraphSyncer> itemSyncers,
            Logger<CloneGraphSync> logger
            )
        {
            _itemSyncers = itemSyncers.OrderByDescending(s => s.Priority);
            _logger = logger;
        }

        public async Task MutateOnClone(ContentItem contentItem)
        {
            var context = new CloneContext(contentItem);

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
