using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Queries.Models;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts.FlowPart
{
    public class FlowMetadataSyncer : IContentPartGraphSyncer
    {
        public string? PartName => "FlowMetaData";

        private const string Alignment = "Alignment";
        private const string Size = "Size";

        public async Task AddSyncComponents(
            dynamic content,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            ContentTypePartDefinition contentTypePartDefinition,
            IGraphSyncHelper graphSyncHelper)
        {
            await AddProperty(Alignment, content, mergeNodeCommand, graphSyncHelper);
            await AddProperty(Size, content, mergeNodeCommand, graphSyncHelper);
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

        private async Task AddProperty(
            string property,
            dynamic content,
            IMergeNodeCommand mergeNodeCommand,
            IGraphSyncHelper graphSyncHelper)
        {
            string propertyName = await graphSyncHelper!.PropertyName(property);
            JValue value = content[property];
            mergeNodeCommand.Properties.Add(propertyName, (int)value);
        }
    }
}
