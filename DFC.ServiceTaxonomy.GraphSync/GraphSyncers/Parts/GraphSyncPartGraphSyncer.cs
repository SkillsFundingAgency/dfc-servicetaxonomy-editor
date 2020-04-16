using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts
{
    public class GraphSyncPartGraphSyncer : IContentPartGraphSyncer
    {
        public string? PartName => nameof(GraphSyncPart);

        public Task AddSyncComponents(
            dynamic content,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            ContentTypePartDefinition contentTypePartDefinition,
            IGraphSyncHelper graphSyncHelper)
        {
            mergeNodeCommand.Properties.Add(graphSyncHelper.IdPropertyName(), graphSyncHelper.GetIdPropertyValue(content));

            return Task.CompletedTask;
        }

        public Task<(bool verified, string failureReason)> VerifySyncComponent(
            JObject content,
            ContentTypePartDefinition contentTypePartDefinition,
            INode sourceNode,
            IEnumerable<IRelationship> relationships,
            IEnumerable<INode> destinationNodes,
            IGraphSyncHelper graphSyncHelper,
            IGraphValidationHelper graphValidationHelper,
            IDictionary<string, int> expectedRelationshipCounts)
        {
            return Task.FromResult(graphValidationHelper.StringContentPropertyMatchesNodeProperty(
                graphSyncHelper.ContentIdPropertyName,
                content,
                graphSyncHelper.IdPropertyName(),
                sourceNode));
        }
    }
}
