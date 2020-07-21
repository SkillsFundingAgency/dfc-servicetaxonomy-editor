using System;
using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.EmbeddedContentItemsGraphSyncer;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
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

        protected override IEnumerable<string> GetEmbeddableContentTypes(
            ContentItem contentItem,
            ContentTypePartDefinition contentTypePartDefinition)
        {
            return new string[]
            {
                contentItem.Content[nameof(TaxonomyPart)][TaxonomyPartGraphSyncer.TermContentTypePropertyName]
            };
        }
    }
}
