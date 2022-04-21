using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.CosmosDb.DataSyncers.Helpers;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.EmbeddedContentItemsDataSyncer;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Results.AllowSync;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Parts.Taxonomy;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Results.AllowSync;
using DFC.ServiceTaxonomy.Taxonomies.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata;

namespace DFC.ServiceTaxonomy.DataSync.CosmosDb.DataSyncers.Parts.Taxonomy
{
    public class CosmosDbTaxonomyPartEmbeddedContentItemsDataSyncer : CosmosDbEmbeddedContentItemsDataSyncer, ITaxonomyPartEmbeddedContentItemsDataSyncer
    {
        public CosmosDbTaxonomyPartEmbeddedContentItemsDataSyncer(
            IContentDefinitionManager contentDefinitionManager,
            //todo: put one in the context??
            ISyncNameProvider statelessSyncNameProvider,
            IServiceProvider serviceProvider,
            ILogger<CosmosDbTaxonomyPartEmbeddedContentItemsDataSyncer> logger)
            : base(contentDefinitionManager, statelessSyncNameProvider, serviceProvider, logger)
        {
        }

        protected override IEnumerable<string> GetEmbeddableContentTypes(IDataSyncContext context)
        {
            IDataSyncContext rootContext = context;
            while (rootContext.ParentContext != null)
            {
                rootContext = rootContext.ParentContext;
            }

            return new string[]
            {
                rootContext.ContentItem.Content[nameof(TaxonomyPart)][
                    TaxonomyPartDataSyncer.TermContentTypePropertyName]
            };
        }

        public bool IsNonLeafEmbeddedTerm { get; set; }

        protected override async Task<string?> TwoWayIncomingRelationshipType(string contentType)
        {
            return IsNonLeafEmbeddedTerm ? $"{await RelationshipType(contentType)}Parent" : null;
        }

        public override async Task AllowSync(
            JArray? contentItems,
            IDataMergeContext context,
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
