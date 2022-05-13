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
using Microsoft.Extensions.Logging;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts.Taxonomy
{
    public class TaxonomyPartEmbeddedContentItemsGraphSyncer : EmbeddedContentItemsGraphSyncer,
        ITaxonomyPartEmbeddedContentItemsGraphSyncer
    {
        public TaxonomyPartEmbeddedContentItemsGraphSyncer(
            IContentDefinitionManager contentDefinitionManager,
            //todo: put one in the context??
            ISyncNameProvider statelessSyncNameProvider,
            IServiceProvider serviceProvider,
            ILogger<TaxonomyPartEmbeddedContentItemsGraphSyncer> logger)
            : base(contentDefinitionManager, statelessSyncNameProvider, serviceProvider, logger)
        {
        }

        protected override IEnumerable<string> GetEmbeddableContentTypes(IGraphSyncContext context)
        {
            IGraphSyncContext rootContext = context;
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

        protected override async Task<string?> TwoWayIncomingRelationshipType(string contentType)
        {
            return IsNonLeafEmbeddedTerm ? $"{await RelationshipType(contentType)}Parent" : null;
        }

        public override async Task AllowSync(
            JArray? contentItems,
            IGraphMergeContext context,
            IAllowSync allowSync)
        {
            IAllowSync baseAllowSync = new AllowSync();
            await base.AllowSync(contentItems, context, baseAllowSync);
            var baseBlockersWithoutIncomingTaxonomyBlocker =
                baseAllowSync.SyncBlockers.Where(sb => sb.ContentType != "Taxonomy");
            if (baseBlockersWithoutIncomingTaxonomyBlocker.Any())
            {
                allowSync.AddSyncBlockers(baseBlockersWithoutIncomingTaxonomyBlocker);
            }
        }
    }
}
