using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Queries.Models;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts
{
    public class GraphSyncPartGraphSyncer : IContentPartGraphSyncer
    {
        public string PartName => nameof(GraphSyncPart);

        public Task AddSyncComponents(JObject content,
            ContentItem contentItem,
            IContentManager contentManager,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            ContentTypePartDefinition contentTypePartDefinition,
            IGraphSyncHelper graphSyncHelper)
        {
            object? idValue = graphSyncHelper.GetIdPropertyValue(content);
            if (idValue != null)
                mergeNodeCommand.Properties.Add(graphSyncHelper.IdPropertyName(), idValue);

            return Task.CompletedTask;
        }

        public Task<(bool validated, string failureReason)> ValidateSyncComponent(JObject content,
            ContentTypePartDefinition contentTypePartDefinition,
            IContentManager contentManager,
            INodeWithOutgoingRelationships nodeWithOutgoingRelationships,
            IGraphSyncHelper graphSyncHelper,
            IGraphValidationHelper graphValidationHelper,
            IDictionary<string, int> expectedRelationshipCounts,
            IValidateAndRepairGraph validateAndRepairGraph)
        {
            return Task.FromResult(graphValidationHelper.StringContentPropertyMatchesNodeProperty(
                graphSyncHelper.ContentIdPropertyName,
                content,
                graphSyncHelper.IdPropertyName(),
                nodeWithOutgoingRelationships.SourceNode));
        }
    }
}
