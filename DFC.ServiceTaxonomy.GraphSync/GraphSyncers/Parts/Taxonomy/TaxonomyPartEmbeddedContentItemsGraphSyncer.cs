using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.EmbeddedContentItemsGraphSyncer;
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

        protected override IEnumerable<string> GetEmbeddableContentTypes(IGraphMergeContext context)
        {
            IGraphMergeContext rootContext = context;
            while (rootContext.ParentGraphMergeContext != null)
            {
                rootContext = rootContext.ParentGraphMergeContext;
            }

            return new string[]
            {
                rootContext.ContentItem.Content[nameof(TaxonomyPart)][
                    TaxonomyPartGraphSyncer.TermContentTypePropertyName]
            };
        }

        public bool IsRoot { get; set; }

        protected override async Task<string?> TwoWayIncomingRelationshipType(
            IGraphSyncHelper embeddedContentGraphSyncHelper)
        {
            return IsRoot ? null : $"{await RelationshipType(embeddedContentGraphSyncHelper)}Parent";
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
