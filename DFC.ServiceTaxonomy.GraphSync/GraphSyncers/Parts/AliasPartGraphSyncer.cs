using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Queries.Models;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using OrchardCore.Alias.Models;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts
{
    public class AliasPartGraphSyncer : IContentPartGraphSyncer
    {
        public string PartName => nameof(AliasPart);

        //todo: configurable??
        private const string _nodePathPropertyName = "alias_alias";

        public Task AddSyncComponents(
            dynamic content,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            ContentTypePartDefinition contentTypePartDefinition,
            IGraphSyncHelper graphSyncHelper)
        {
            JValue pathValue = content.Alias;
            if (pathValue.Type != JTokenType.Null)
                mergeNodeCommand.Properties.Add(_nodePathPropertyName, pathValue.As<string>());

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
            return Task.FromResult(graphValidationHelper.StringContentPropertyMatchesNodeProperty(
                "Alias",
                content,
                _nodePathPropertyName,
                nodeWithOutgoingRelationships.SourceNode));
        }
    }
}
