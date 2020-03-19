using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Exceptions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
using DFC.ServiceTaxonomy.Neo4j.Commands;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Flows.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts
{
    public class BagPartGraphSyncer : IContentPartGraphSyncer
    {
        private readonly IServiceProvider _serviceProvider;

        public string? PartName => nameof(BagPart);

        public BagPartGraphSyncer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<IEnumerable<ICommand>> AddSyncComponents(
            dynamic graphLookupContent,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            ContentTypePartDefinition contentTypePartDefinition,
            IGraphSyncHelper graphSyncHelper)
        {
            var delayedCommands = new List<ICommand>();

            foreach (JObject? contentItem in graphLookupContent.ContentItems)
            {
                var mergeGraphSyncer = _serviceProvider.GetRequiredService<IMergeGraphSyncer>();

                string contentType = contentItem!["ContentType"]!.ToString();
                string contentItemId = contentItem!["ContentItemId"]!.ToString();
                string contentItemVersionId = contentItem!["ContentItemVersionId"]!.ToString();

                DateTime? createdDate = !string.IsNullOrEmpty(contentItem["CreatedUtc"]!.ToString()) ? DateTime.Parse(contentItem["CreatedUtc"]!.ToString()) : (DateTime?)null;
                DateTime? modifiedDate = !string.IsNullOrEmpty(contentItem["ModifiedUtc"]!.ToString()) ? DateTime.Parse(contentItem["ModifiedUtc"]!.ToString()) : (DateTime?)null;

                //todo: if we want to support nested bags, would have to return queries also
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

                var delayedReplaceRelationshipsCommand = _serviceProvider.GetRequiredService<IReplaceRelationshipsCommand>();
                delayedReplaceRelationshipsCommand.SourceNodeLabels = new HashSet<string>(mergeNodeCommand.NodeLabels);

                if (mergeNodeCommand.IdPropertyName == null)
                    throw new GraphSyncException($"Supplied merge node command has null {nameof(mergeNodeCommand.IdPropertyName)}");
                delayedReplaceRelationshipsCommand.SourceIdPropertyName = mergeNodeCommand.IdPropertyName;
                delayedReplaceRelationshipsCommand.SourceIdPropertyValue = (string?)mergeNodeCommand.Properties[delayedReplaceRelationshipsCommand.SourceIdPropertyName];

                //todo: move this code into graphSyncHelper
                //todo: what if want different relationships for same contenttype in different bags!
                string? relationshipType = graphSyncHelper.GraphSyncPartSettings.BagPartContentItemRelationshipType;
                if (string.IsNullOrEmpty(relationshipType))
                    relationshipType = await graphSyncHelper.RelationshipType(contentType);

                delayedReplaceRelationshipsCommand.AddRelationshipsTo(
                    relationshipType,
                    containedContentMergeNodeCommand.NodeLabels,
                    containedContentMergeNodeCommand.IdPropertyName!,
                    containedContentMergeNodeCommand.Properties[containedContentMergeNodeCommand.IdPropertyName!]);

                delayedCommands.Add(delayedReplaceRelationshipsCommand);
            }

            return delayedCommands;
        }

        public async Task<bool> VerifySyncComponent(
            ContentItem contentItem,
            ContentTypePartDefinition contentTypePartDefinition,
            INode sourceNode,
            IEnumerable<IRelationship> relationships,
            IEnumerable<INode> destNodes)
        {
            var graphSyncValidator = _serviceProvider.GetRequiredService<IGraphSyncValidator>();

            var contentItems = contentItem.Content[contentTypePartDefinition.Name]["ContentItems"].ToObject<IEnumerable<ContentItem>>();

            foreach (var bagPartContentItem in contentItems)
            {
                //todo: foreach will never iterate
                if (await graphSyncValidator.CheckIfContentItemSynced(bagPartContentItem))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
    }
}
