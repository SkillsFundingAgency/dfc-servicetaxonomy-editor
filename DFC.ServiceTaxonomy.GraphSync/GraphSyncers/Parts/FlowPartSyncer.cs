using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Extensions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Exceptions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Queries.Models;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
using DFC.ServiceTaxonomy.Neo4j.Commands;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Flows.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts
{
    //todo: there's an opportunity to share code with bagpart

#pragma warning disable S1481 // need the variable for the new using syntax, see https://github.com/dotnet/csharplang/issues/2235

    public class FlowPartGraphSyncer : IContentPartGraphSyncer
    {
        private readonly IContentFieldsGraphSyncer _contentFieldsGraphSyncer;
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, ContentTypeDefinition> _contentTypes;

        public string PartName => nameof(FlowPart);

        private const string Widgets = "Widgets";
        private const string FlowMetaData = "FlowMetadata";
        private const string Alignment = "Alignment";
        private const string Size = "Size";
        private const string Ordinal = "Ordinal";

        private static Func<string, string> _flowFieldsPropertyNameTransform = n => $"flow_{n}";

        public FlowPartGraphSyncer(
            IContentDefinitionManager contentDefinitionManager,
            IContentFieldsGraphSyncer contentFieldsGraphSyncer,
            IServiceProvider serviceProvider)
        {
            _contentFieldsGraphSyncer = contentFieldsGraphSyncer;
            _serviceProvider = serviceProvider;

            _contentTypes = contentDefinitionManager
                .ListTypeDefinitions()
                .Where(x => x.Parts.Any(p => p.Name == nameof(GraphSyncPart)))
                .ToDictionary(x => x.Name);
        }

        public async Task AddSyncComponents(
            dynamic content,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            ContentTypePartDefinition contentTypePartDefinition,
            IGraphSyncHelper graphSyncHelper)
        {
            await AddWidgetsSyncComponents(content, replaceRelationshipsCommand, graphSyncHelper);

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

        private async Task AddWidgetsSyncComponents(
            dynamic content,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            IGraphSyncHelper graphSyncHelper)
        {
            int widgetOrdinal = 0;
            foreach (JObject? contentItem in content[Widgets])
            {
                //todo: share code with bag part?
                var mergeGraphSyncer = _serviceProvider.GetRequiredService<IMergeGraphSyncer>();

                string contentType = contentItem!["ContentType"]!.ToString();
                string contentItemId = contentItem!["ContentItemId"]!.ToString();
                string contentItemVersionId = contentItem!["ContentItemVersionId"]!.ToString();
                DateTime? createdDate = contentItem.GetDateTime("CreatedUtc");
                DateTime? modifiedDate = contentItem.GetDateTime("ModifiedUtc");

                //todo: if we want to support nested flows, would have to return queries also
                IMergeNodeCommand? containedContentMergeNodeCommand = await mergeGraphSyncer.SyncToGraph(
                    contentType,
                    contentItemId,
                    contentItemVersionId,
                    contentItem!,
                    createdDate,
                    modifiedDate);
                // if the contained content type wasn't synced (i.e. it doesn't have a graph sync part), then there's nothing to create a relationship to
                if (containedContentMergeNodeCommand == null)
                    continue;

                containedContentMergeNodeCommand.CheckIsValid();

                var relationshipProperties = await GetFlowMetaData(contentItem, widgetOrdinal++, graphSyncHelper);

                await AddRelationshipToContainedContent(replaceRelationshipsCommand, contentType, relationshipProperties,
                    containedContentMergeNodeCommand);
            }
        }

        private async Task<Dictionary<string, object>> GetFlowMetaData(
            JObject contentItem,
            int ordinal,
            IGraphSyncHelper graphSyncHelper)
        {
            var flowMetaData = new Dictionary<string, object>
            {
                {Ordinal, (long)ordinal}
            };

            //todo: do we need more config/method for RelationshipPropertyName (and rename existing NodePropertyName?)
            //todo: handle nulls?

            JObject flowMetaDataContent = (JObject)contentItem[FlowMetaData]!;

            FlowAlignment alignment = (FlowAlignment)(int)flowMetaDataContent[Alignment]!;
            flowMetaData.Add(await graphSyncHelper!.PropertyName(Alignment), alignment.ToString());

            flowMetaData.Add(await graphSyncHelper!.PropertyName(Size), (long)flowMetaDataContent[Size]!);

            return flowMetaData;
        }

        //extension on IReplaceRelationshipsCommand?
        private async Task AddRelationshipToContainedContent(
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            string contentType,
            Dictionary<string, object>? relationshipProperties,
            IMergeNodeCommand containedContentMergeNodeCommand)
        {
            var flowContentItemGraphSyncHelper = _serviceProvider.GetRequiredService<IGraphSyncHelper>();

            flowContentItemGraphSyncHelper.ContentType = contentType;
            string relationshipType = await RelationshipType(flowContentItemGraphSyncHelper);

            replaceRelationshipsCommand.AddRelationshipsTo(
                relationshipType,
                relationshipProperties,
                containedContentMergeNodeCommand.NodeLabels,
                containedContentMergeNodeCommand.IdPropertyName!,
                containedContentMergeNodeCommand.Properties[containedContentMergeNodeCommand.IdPropertyName!]);
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
            (bool validated, string failureReason) = await ValidateWidgetsSyncComponent(
                content,
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

        private async Task<(bool validated, string failureReason)> ValidateWidgetsSyncComponent(
            JObject content,
            INodeWithOutgoingRelationships nodeWithOutgoingRelationships,
            IGraphValidationHelper graphValidationHelper,
            IDictionary<string, int> expectedRelationshipCounts,
            string endpoint)
        {
            IEnumerable<ContentItem>? contentItems = content[Widgets]?.ToObject<IEnumerable<ContentItem>>();
            if (contentItems == null)
                throw new GraphSyncException("Flow does not contain Widgets");

            int widgetOrdinal = 0;
            foreach (ContentItem flowPartContentItem in contentItems)
            {
                var graphSyncValidator = _serviceProvider.GetRequiredService<IValidateAndRepairGraph>();

                ContentTypeDefinition flowPartContentTypeDefinition = _contentTypes[flowPartContentItem.ContentType];

                (bool validated, string failureReason) =
                    await graphSyncValidator.ValidateContentItem(flowPartContentItem, flowPartContentTypeDefinition,
                        endpoint);

                if (!validated)
                    return (false, $"contained item failed validation: {failureReason}");

                // check expected relationship is in graph
                var flowContentGraphSyncHelper = _serviceProvider.GetRequiredService<IGraphSyncHelper>();

                flowContentGraphSyncHelper.ContentType = flowPartContentItem.ContentType;
                string expectedRelationshipType = await RelationshipType(flowContentGraphSyncHelper);

                // keep a count of how many relationships of a type we expect to be in the graph
                expectedRelationshipCounts.TryGetValue(expectedRelationshipType, out int currentCount);
                expectedRelationshipCounts[expectedRelationshipType] = ++currentCount;

                // we've already validated the destination node, so we can assume the id property is there
                object destinationId =
                    flowContentGraphSyncHelper.GetIdPropertyValue(flowPartContentItem.Content.GraphSyncPart);

                string flowContentIdPropertyName =
                    flowContentGraphSyncHelper.IdPropertyName(flowPartContentItem.ContentType);

                var expectedRelationshipProperties = await GetFlowMetaData(
                    (JObject)flowPartContentItem.Content, widgetOrdinal++, flowContentGraphSyncHelper);

                (validated, failureReason) = graphValidationHelper.ValidateOutgoingRelationship(
                    nodeWithOutgoingRelationships,
                    expectedRelationshipType,
                    flowContentIdPropertyName,
                    destinationId,
                    expectedRelationshipProperties);

                if (!validated)
                    return (false, failureReason);
            }

            return (true, "");
        }

        private async Task<string> RelationshipType(IGraphSyncHelper graphSyncHelper)
        {
            //todo: configurable?
            return await graphSyncHelper.RelationshipTypeDefault(graphSyncHelper.ContentType!);
        }
    }

#pragma warning restore S1481
}
