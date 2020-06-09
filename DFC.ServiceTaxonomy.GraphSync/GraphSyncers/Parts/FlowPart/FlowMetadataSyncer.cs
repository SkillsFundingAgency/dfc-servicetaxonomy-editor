using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Queries.Models;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Flows.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts.FlowPart
{
    public class FlowMetadataSyncer : IContentPartGraphSyncer
    {
        public string PartName => "FlowMetaData";

        private const string Alignment = "Alignment";
        private const string Size = "Size";

        public async Task AddSyncComponents(
            dynamic content,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            ContentTypePartDefinition contentTypePartDefinition,
            IGraphSyncHelper graphSyncHelper)
        {
            FlowAlignment alignment = (FlowAlignment)(int)content[Alignment];
            mergeNodeCommand.Properties.Add(await graphSyncHelper!.PropertyName(Alignment), alignment.ToString());

            mergeNodeCommand.Properties.Add(await graphSyncHelper!.PropertyName(Size), (int)content[Size]);
        }

        public Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject content,
            ContentTypePartDefinition contentTypePartDefinition,
            INodeWithOutgoingRelationships nodeWithOutgoingRelationships,
            IGraphSyncHelper graphSyncHelper,
            IGraphValidationHelper graphValidationHelper,
            IDictionary<string, int> expectedRelationshipCounts,
            string endpoint)
        {
            throw new NotImplementedException();
        }
    }
}
