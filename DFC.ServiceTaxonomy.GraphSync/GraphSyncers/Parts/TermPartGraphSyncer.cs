using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Queries.Models;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Taxonomies.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts
{
    public class TermPartGraphSyncer : IContentPartGraphSyncer
    {
        public string PartName => nameof(TermPart);

        public Task AddSyncComponents(
            dynamic content,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            ContentTypePartDefinition contentTypePartDefinition,
            IGraphSyncHelper graphSyncHelper)
        {
            return Task.CompletedTask;
        }

        public Task<(bool validated, string failureReason)> ValidateSyncComponent(JObject content,
            ContentTypePartDefinition contentTypePartDefinition,
            INodeWithOutgoingRelationships nodeWithOutgoingRelationships,
            IGraphSyncHelper graphSyncHelper,
            IGraphValidationHelper graphValidationHelper,
            IDictionary<string, int> expectedRelationshipCounts,
            string endpoint)
        {
            return Task.FromResult((true, ""));
        }
    }
}
