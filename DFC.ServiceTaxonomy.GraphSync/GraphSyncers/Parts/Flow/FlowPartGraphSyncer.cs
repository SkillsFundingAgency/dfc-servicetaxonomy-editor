using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.EmbeddedContentItemsGraphSyncer;
using DFC.ServiceTaxonomy.GraphSync.Queries.Models;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Flows.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts.Flow
{
#pragma warning disable S1481 // need the variable for the new using syntax, see https://github.com/dotnet/csharplang/issues/2235

    public class FlowPartGraphSyncer : IContentPartGraphSyncer
    {
        private readonly IFlowPartEmbeddedContentItemsGraphSyncer _flowPartEmbeddedContentItemsGraphSyncer;
        private readonly IContentFieldsGraphSyncer _contentFieldsGraphSyncer;

        public string PartName => nameof(FlowPart);

        private const string ContainerName = "Widgets";
        private static readonly Func<string, string> _flowFieldsPropertyNameTransform = n => $"flow_{n}";

        public FlowPartGraphSyncer(
            IFlowPartEmbeddedContentItemsGraphSyncer flowPartEmbeddedContentItemsGraphSyncer,
            IContentFieldsGraphSyncer contentFieldsGraphSyncer)
        {
            _flowPartEmbeddedContentItemsGraphSyncer = flowPartEmbeddedContentItemsGraphSyncer;
            _contentFieldsGraphSyncer = contentFieldsGraphSyncer;
        }

        public async Task AddSyncComponents(
            dynamic content,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            ContentTypePartDefinition contentTypePartDefinition,
            IGraphSyncHelper graphSyncHelper)
        {
            await _flowPartEmbeddedContentItemsGraphSyncer.AddSyncComponents(
                content[ContainerName],
                replaceRelationshipsCommand,
                graphSyncHelper);

            // FlowPart allows part definition level fields, but values are on each FlowPart instance
            // prefix flow field property names, so there's no possibility of a clash with the eponymous fields property names
            using var _ = graphSyncHelper.PushPropertyNameTransform(_flowFieldsPropertyNameTransform);

            await _contentFieldsGraphSyncer.AddSyncComponents(
                content,
                mergeNodeCommand,
                replaceRelationshipsCommand,
                contentTypePartDefinition,
                graphSyncHelper);
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
            (bool validated, string failureReason) =
                await _flowPartEmbeddedContentItemsGraphSyncer.ValidateSyncComponent(
                    (JArray?)content[ContainerName],
                    nodeWithOutgoingRelationships,
                    graphValidationHelper,
                    expectedRelationshipCounts,
                    endpoint);

            if (!validated)
                return (validated, failureReason);

            using var _ = graphSyncHelper.PushPropertyNameTransform(_flowFieldsPropertyNameTransform);

            (validated, failureReason) = await _contentFieldsGraphSyncer.ValidateSyncComponent(
                content,
                contentTypePartDefinition,
                nodeWithOutgoingRelationships,
                graphSyncHelper,
                graphValidationHelper,
                expectedRelationshipCounts);

            return (validated, failureReason);
        }
    }

#pragma warning restore S1481
}
