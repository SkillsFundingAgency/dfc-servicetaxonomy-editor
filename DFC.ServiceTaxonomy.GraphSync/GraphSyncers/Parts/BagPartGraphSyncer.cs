using System;
using System.Collections.Generic;
using System.Linq;
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
            dynamic content,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            ContentTypePartDefinition contentTypePartDefinition,
            IGraphSyncHelper graphSyncHelper)
        {
            var delayedCommands = new List<ICommand>();

            foreach (JObject? contentItem in content.ContentItems)
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

                graphSyncHelper.ContentType = contentType;
                string relationshipType = await RelationshipType(graphSyncHelper);

                delayedReplaceRelationshipsCommand.AddRelationshipsTo(
                    relationshipType,
                    containedContentMergeNodeCommand.NodeLabels,
                    containedContentMergeNodeCommand.IdPropertyName!,
                    containedContentMergeNodeCommand.Properties[containedContentMergeNodeCommand.IdPropertyName!]);

                delayedCommands.Add(delayedReplaceRelationshipsCommand);
            }

            return delayedCommands;
        }

        //todo: in all verifies, log reason verification fails
        public async Task<bool> VerifySyncComponent(
            dynamic content,
            ContentTypePartDefinition contentTypePartDefinition,
            INode sourceNode,
            IEnumerable<IRelationship> relationships,
            IEnumerable<INode> destNodes,
            IGraphSyncHelper graphSyncHelper)
        {
            IEnumerable<ContentItem> contentItems = content["ContentItems"].ToObject<IEnumerable<ContentItem>>();

            Dictionary<string, int> expectedRelationshipCounts = new Dictionary<string, int>();

            foreach (ContentItem bagPartContentItem in contentItems)
            {
                var graphSyncValidator = _serviceProvider.GetRequiredService<IGraphSyncValidator>();

                if (!await graphSyncValidator.CheckIfContentItemSynced(bagPartContentItem))
                    return false;

                // check expected relationship is in graph
                var bagContentGraphSyncValidator = _serviceProvider.GetRequiredService<IGraphSyncHelper>();

                //todo: check elsewhere where we set the graphsynchelper's content type (it will hang around)
                bagContentGraphSyncValidator.ContentType = bagPartContentItem.ContentType;
                string expectedRelationship = await RelationshipType(bagContentGraphSyncValidator);

                // keep a count of how many relationships of a type we expect to be in the graph
                expectedRelationshipCounts.TryGetValue(expectedRelationship, out int currentCount);
                expectedRelationshipCounts[expectedRelationship] = ++currentCount;

                //todo: check relationships dest id and dest nodes
            }

            // check there aren't any more relationships of each type than there should be
            foreach ((string relationship, int relationshipsInDbCount) in expectedRelationshipCounts)
            {
                int relationshipsInGraphCount = relationships.Count(r => string.Equals(r.Type, relationship, StringComparison.CurrentCultureIgnoreCase));

                if (relationshipsInDbCount != relationshipsInGraphCount)
                    return false;
            }

            return true;
        }

        private async Task<string> RelationshipType(IGraphSyncHelper graphSyncHelper)
        {
            //todo: what if want different relationships for same contenttype in different bags!
            string? relationshipType = graphSyncHelper.GraphSyncPartSettings.BagPartContentItemRelationshipType;
            if (string.IsNullOrEmpty(relationshipType))
                relationshipType = await graphSyncHelper.RelationshipTypeDefault(graphSyncHelper.ContentType!);

            return relationshipType;
        }
    }
}
