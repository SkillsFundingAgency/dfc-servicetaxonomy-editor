using System;
using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Exceptions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.EmbeddedContentItemsGraphSyncer;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts.Taxonomy
{
    public class TaxonomyPartEmbeddedContentItemsGraphSyncer : EmbeddedContentItemsGraphSyncer, ITaxonomyPartEmbeddedContentItemsGraphSyncer
    {
        public TaxonomyPartEmbeddedContentItemsGraphSyncer(
            IContentDefinitionManager contentDefinitionManager,
            IServiceProvider serviceProvider)
            : base(contentDefinitionManager, serviceProvider)
        {
        }

        protected override IEnumerable<ContentItem> ValidateSyncComponentConvertToContentItems(JArray? contentItems)
        {
            if (contentItems == null)
            {
                // we've seen this when the import util generates bad data. we fail fast
                throw new GraphSyncException("Embedded content container has missing array.");
            }

            IEnumerable<ContentItem>? embeddedContentItems = contentItems.ToObject<IEnumerable<ContentItem>>();
            if (embeddedContentItems == null)
                throw new GraphSyncException("Embedded content container does not contain ContentItems.");

            return GetFlattenedTermsContentItems(embeddedContentItems);
        }

        private static ContentItem[] GetFlattenedTermsContentItems(IEnumerable<ContentItem> termsRootContentItems)
        {
            return termsRootContentItems
                .Flatten(ci => ((JArray?)((JObject)ci.Content)["Terms"])
                    ?.ToObject<IEnumerable<ContentItem>>() ?? Enumerable.Empty<ContentItem>())
                .ToArray();
        }
    }
}
