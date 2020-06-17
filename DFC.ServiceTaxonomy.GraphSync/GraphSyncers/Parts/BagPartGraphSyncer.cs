using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Queries.Models;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Flows.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts
{
    public class BagPartGraphSyncer : IContentPartGraphSyncer
    {
        private readonly IBagPartEmbeddedContentItemsGraphSyncer _bagPartEmbeddedContentItemsGraphSyncer;
        public string PartName => nameof(BagPart);

        public BagPartGraphSyncer(IBagPartEmbeddedContentItemsGraphSyncer bagPartEmbeddedContentItemsGraphSyncer)
        {
            _bagPartEmbeddedContentItemsGraphSyncer = bagPartEmbeddedContentItemsGraphSyncer;
        }

        public async Task AddSyncComponents(
            dynamic content,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            ContentTypePartDefinition contentTypePartDefinition,
            IGraphSyncHelper graphSyncHelper)
        {
            await _bagPartEmbeddedContentItemsGraphSyncer.AddSyncComponents(content, replaceRelationshipsCommand);
        }

        public async Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject content,
            ContentTypePartDefinition contentTypePartDefinition,
            INodeWithOutgoingRelationships nodeWithOutgoingRelationships,
            IGraphSyncHelper graphSyncHelper,
            IGraphValidationHelper graphValidationHelper,
            IDictionary<string, int> expectedRelationshipCounts,
            string endpoint)
        {
            return await _bagPartEmbeddedContentItemsGraphSyncer.ValidateSyncComponent(
                content,
                nodeWithOutgoingRelationships,
                graphValidationHelper,
                expectedRelationshipCounts,
                endpoint);
        }
    }
}
