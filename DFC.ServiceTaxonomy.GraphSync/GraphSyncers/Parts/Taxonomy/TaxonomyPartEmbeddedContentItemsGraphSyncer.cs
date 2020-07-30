using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.EmbeddedContentItemsGraphSyncer;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Taxonomies.Models;

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

        protected override IEnumerable<string> GetEmbeddableContentTypes(IGraphMergeContext context)
        {
            IGraphMergeContext rootContext = context;
            while (rootContext.ParentGraphMergeContext != null)
            {
                rootContext = rootContext.ParentGraphMergeContext;
            }

            return new string[]
            {
                rootContext.ContentItem.Content[nameof(TaxonomyPart)][TaxonomyPartGraphSyncer.TermContentTypePropertyName]
            };
        }

        public bool IsRoot { get; set; }

        protected override async Task<string?> TwoWayIncomingRelationshipType(IGraphSyncHelper embeddedContentGraphSyncHelper)
        {
            return IsRoot ? null : $"{await RelationshipType(embeddedContentGraphSyncHelper)}Parent";
        }
    }
}
