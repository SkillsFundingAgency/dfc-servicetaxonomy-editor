using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    //todo:
    // unit/integration tests for rr command tests
    /*
{
  "Page": {},
  "FlowPart": {
    "Widgets": [
      {
        "ContentItemId": "4sqvcvbm1pst0xvpcm05800cc6",
        "ContentItemVersionId": null,
        "ContentType": "SharedContentWidget",
        "DisplayText": null,
        "Latest": false,
        "Published": false,
        "ModifiedUtc": "2020-06-09T11:12:04.2853264Z",
        "PublishedUtc": null,
        "CreatedUtc": null,
        "Owner": null,
        "Author": "admin",
        "SharedContentWidget": {    <- eponymous part
          "SharedContent": {        <- content picker field
            "ContentItemIds": [
              "4kqbfyyy1ahd5xndf45fg2rad0",
              "4qaxdrfrj52q06re6n4jmg5mmb"
            ]
          }
        },
        "GraphSyncPart": {
          "Text": "c46449b2-28ed-4061-bd22-b33b0a7ec5e9"
        },
        "FlowMetadata": {
          "Alignment": 3,
          "Size": 33
        }
      },
      {
        "ContentItemId": "44ayqx3r3f15r1346we91623mr",
        "ContentItemVersionId": null,
        "ContentType": "Html",
        "DisplayText": null,
        "Latest": false,
        "Published": false,
        "ModifiedUtc": "2020-06-09T11:12:04.4036209Z",
        "PublishedUtc": null,
        "CreatedUtc": null,
        "Owner": null,
        "Author": "admin",
        "Html": {        <- eponymous part of widget
          "texty": {
            "Text": "the text"
          }
        },
        "HtmlBodyPart": {
          "Html": "<p>html here</p>"
        },
        "FlowMetadata": {
          "Alignment": 3,
          "Size": 33
        }
      }
    ],
    "flowfield": {        <= part definition level (common for all flow parts)
      "Text": null
    }
  },
  "TitlePart": {
    "Title": "sharedcontentpage"
  },
  "SitemapPart": {
    "Priority": 5,
    "OverrideSitemapConfig": false,
    "ChangeFrequency": 0,
    "Exclude": false
  },
  "GraphSyncPart": {
    "Text": "19572fc1-f8b4-4265-8303-ea8eeb3634c7"
  }
}     */

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

            // FlowPart allows part definition level fields, but values are on each instance
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
                var mergeGraphSyncer = _serviceProvider.GetRequiredService<IMergeGraphSyncer>();

                string contentType = contentItem!["ContentType"]!.ToString();
                string contentItemId = contentItem!["ContentItemId"]!.ToString();
                string contentItemVersionId = contentItem!["ContentItemVersionId"]!.ToString();

                //todo: helpers for these. extension on JObject? GetDateTime(string name)
                DateTime? createdDate = !string.IsNullOrEmpty(contentItem["CreatedUtc"]!.ToString())
                    ? DateTime.Parse(contentItem["CreatedUtc"]!.ToString())
                    : (DateTime?) null;
                DateTime? modifiedDate = !string.IsNullOrEmpty(contentItem["ModifiedUtc"]!.ToString())
                    ? DateTime.Parse(contentItem["ModifiedUtc"]!.ToString())
                    : (DateTime?) null;

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

                //todo: we want to add properties to contained widget, but it's already been synced
                // add and resync?
                // add new node for metadata and add relationship?
                //properties on the relationship. nice from graph point of view, but what about content api?

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
            IEnumerable<ContentItem>? contentItems = content[Widgets]?.ToObject<IEnumerable<ContentItem>>();
            if (contentItems == null)
                throw new GraphSyncException("Flow does not contain Widgets");

            int widgetOrdinal = 0;
            foreach (ContentItem flowPartContentItem in contentItems)
            {
                var graphSyncValidator = _serviceProvider.GetRequiredService<IValidateAndRepairGraph>();

                ContentTypeDefinition flowPartContentTypeDefinition = _contentTypes[flowPartContentItem.ContentType];

                (bool validated, string failureReason) =
                    await graphSyncValidator.ValidateContentItem(flowPartContentItem, flowPartContentTypeDefinition, endpoint);

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
                object destinationId = flowContentGraphSyncHelper.GetIdPropertyValue(flowPartContentItem.Content.GraphSyncPart);

                string flowContentIdPropertyName = flowContentGraphSyncHelper.IdPropertyName(flowPartContentItem.ContentType);

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

            return await _contentFieldsGraphSyncer.ValidateSyncComponent(
                content,
                contentTypePartDefinition,
                nodeWithOutgoingRelationships,
                graphSyncHelper,
                graphValidationHelper,
                expectedRelationshipCounts);
        }

        private async Task<string> RelationshipType(IGraphSyncHelper graphSyncHelper)
        {
            //todo: configurable?
            return await graphSyncHelper.RelationshipTypeDefault(graphSyncHelper.ContentType!);
        }
    }
}
