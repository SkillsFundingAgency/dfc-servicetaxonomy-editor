using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.EmbeddedContentItemsGraphSyncer;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Flows.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts.Bag
{
    public class BagPartEmbeddedContentItemsGraphSyncer : EmbeddedContentItemsGraphSyncer, IBagPartEmbeddedContentItemsGraphSyncer
    {
        public BagPartEmbeddedContentItemsGraphSyncer(
            IContentDefinitionManager contentDefinitionManager,
            IServiceProvider serviceProvider)
            : base(contentDefinitionManager, serviceProvider)
        {
        }
        // nothing should be creating incoming relationships to embedded bag items, so we shortcut the check
        // if we do start adding incoming relationships, we should let the base class do its stuff
        public override Task AllowSync(JArray? contentItems, IGraphMergeContext context,
            IAllowSyncResult allowSyncResult) => Task.FromResult(true);

        protected override IEnumerable<string> GetEmbeddableContentTypes(IGraphMergeContext context)
        {
            BagPartSettings bagPartSettings = context.ContentTypePartDefinition.GetSettings<BagPartSettings>();
            return bagPartSettings.ContainedContentTypes;
        }

        protected override async Task<string> RelationshipType(IGraphSyncHelper embeddedContentGraphSyncHelper)
        {
            //todo: what if want different relationships for same contenttype in different bags!
            string? relationshipType = embeddedContentGraphSyncHelper.GraphSyncPartSettings.BagPartContentItemRelationshipType;
            if (string.IsNullOrEmpty(relationshipType))
                relationshipType = await base.RelationshipType(embeddedContentGraphSyncHelper);

            return relationshipType;
        }
    }
}
