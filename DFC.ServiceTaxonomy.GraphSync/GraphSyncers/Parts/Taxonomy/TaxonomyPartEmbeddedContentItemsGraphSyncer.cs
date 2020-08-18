using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.EmbeddedContentItemsGraphSyncer;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.AllowSync;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Results.AllowSync;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata;
using DFC.ServiceTaxonomy.Taxonomies.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts.Taxonomy
{
    public class TaxonomyPartEmbeddedContentItemsGraphSyncer : EmbeddedContentItemsGraphSyncer,
        ITaxonomyPartEmbeddedContentItemsGraphSyncer
    {
        public TaxonomyPartEmbeddedContentItemsGraphSyncer(
            IContentDefinitionManager contentDefinitionManager,
            IServiceProvider serviceProvider)
            : base(contentDefinitionManager, serviceProvider)
        {
        }

        protected override IEnumerable<string> GetEmbeddableContentTypes(IGraphOperationContext context)
        {
            IGraphOperationContext rootContext = context;
            while (rootContext.ParentContext != null)
            {
                rootContext = rootContext.ParentContext;
            }

            return new string[]
            {
                rootContext.ContentItem.Content[nameof(TaxonomyPart)][
                    TaxonomyPartGraphSyncer.TermContentTypePropertyName]
            };
        }

        public bool IsNonLeafEmbeddedTerm { get; set; }

        protected override async Task<string?> TwoWayIncomingRelationshipType(
            IGraphSyncHelper embeddedContentGraphSyncHelper)
        {
            return IsNonLeafEmbeddedTerm ? $"{await RelationshipType(embeddedContentGraphSyncHelper)}Parent" : null;
        }

        public override async Task AllowSync(
            JArray? contentItems,
            IGraphMergeContext context,
            IAllowSyncResult allowSyncResult)
        {
            var baseAllowSyncResult = new AllowSyncResult();
            await base.AllowSync(contentItems, context, baseAllowSyncResult);
            var baseBlockersWithoutIncomingTaxonomyBlocker =
                baseAllowSyncResult.SyncBlockers.Where(sb => sb.ContentType != "Taxonomy");
            if (baseBlockersWithoutIncomingTaxonomyBlocker.Any())
            {
                allowSyncResult.AddSyncBlockers(baseBlockersWithoutIncomingTaxonomyBlocker);
            }
        }
    }
}
