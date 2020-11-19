using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Items;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries;
using DFC.ServiceTaxonomy.GraphSync.Settings;
using DFC.ServiceTaxonomy.Neo4j.Queries;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers
{
    public class DescribeContentItemHelper : IDescribeContentItemHelper
    {
        private readonly IContentManager _contentManager;
        private readonly IEnumerable<IContentItemGraphSyncer> _contentItemGraphSyncers;
        //private readonly List<string> _encounteredContentItems = new List<string>();
        private readonly List<string> _encounteredContentTypes = new List<string>();
        private readonly IOptionsMonitor<GraphSyncSettings> _graphSyncSettings;

        public DescribeContentItemHelper(
            IContentManager contentManager,
            IEnumerable<IContentItemGraphSyncer> contentItemGraphSyncers,
            IOptionsMonitor<GraphSyncSettings> graphSyncSettings)
        {
            _contentItemGraphSyncers = contentItemGraphSyncers.OrderByDescending(s => s.Priority);

            _contentManager = contentManager;
            _graphSyncSettings = graphSyncSettings;
        }

        //todo: only ever called with both contexts the same
        public async Task<IEnumerable<IQuery<object?>>> GetRelationshipCommands(
            IDescribeRelationshipsContext context)
            //IDescribeRelationshipsContext parentContext)
        {
            var currentList = new List<ContentItemRelationship>();

            var allRelationships = await GetRelationships(context, currentList, context);
            var uniqueCommands = allRelationships.Select(z => z.RelationshipPathString).GroupBy(x => x).Select(g => g.First());

            List<IQuery<object?>> commandsToReturn = uniqueCommands
                .Select(c => new NodeAndNestedOutgoingRelationshipsQuery(c!)).Cast<IQuery<object?>>().ToList();

            //todo: for occupation and skill, we need to filter out nodes that have just the skos__Concept and Resource labels (and others)
            // but allow other nodes that have a skos__Concept label, such as occupations and skills
            // (or filter on relationships, whitelist whatever)
            //todo: add a setting to graphsyncsettings for the filtering (for now we'll set incoming to 0 for occs & skills)
            var graphSyncPartSettings = context.SyncNameProvider.GetGraphSyncPartSettings(context.ContentItem.ContentType);

            commandsToReturn.Add(new SubgraphQuery(
                context.SourceNodeLabels,
                context.SourceNodeIdPropertyName,
                context.SourceNodeId,
                SubgraphQuery.RelationshipFilterIncoming,
                graphSyncPartSettings.VisualiserIncomingRelationshipsPathLength ?? 1));

            return commandsToReturn;
        }

        //todo: contentmanager
        //todo: taxonomies use reltype*maxdepth ?
        //todo: for child contexts, do we need anything more than parentcontext, contentitem & relationships?

        public async Task<IDescribeRelationshipsContext?> BuildRelationships(
            ContentItem contentItem,
            string sourceNodeIdPropertyName,
            string sourceNodeId,
            IEnumerable<string> sourceNodeLabels,
            ISyncNameProvider syncNameProvider,
            IContentManager contentManager,
            IContentItemVersion contentItemVersion,
            IDescribeRelationshipsContext? parentContext,
            IServiceProvider serviceProvider,
            ContentItem rootContentItem)
        {
            var graphSyncPartSettings = syncNameProvider.GetGraphSyncPartSettings(contentItem.ContentType);

            int maxDepthFromHere;

            if (parentContext == null)
            {
                maxDepthFromHere = Math.Min(graphSyncPartSettings.VisualiserNodeDepth ?? int.MaxValue,
                    //todo: store in root in case changes mid flow?
                    _graphSyncSettings.CurrentValue.MaxVisualiserNodeDepth);
            }
            else
            {
                if (_encounteredContentTypes.Any(x => x == contentItem.ContentType))
                    return null;

                maxDepthFromHere = Math.Min(parentContext.MaxDepthFromHere - 1,
                    graphSyncPartSettings.VisualiserNodeDepth ?? int.MaxValue);
            }

            if (maxDepthFromHere <= 0)
                return null;

            var context = new DescribeRelationshipsContext(
                sourceNodeIdPropertyName, sourceNodeId, sourceNodeLabels, contentItem, maxDepthFromHere, syncNameProvider,
                contentManager, contentItemVersion, parentContext, serviceProvider, rootContentItem);

            foreach (IContentItemGraphSyncer itemSyncer in _contentItemGraphSyncers)
            {
                //todo: allow syncers to chain or not? probably not
                if (itemSyncer.CanSync(context.ContentItem))
                {
                    await itemSyncer.AddRelationship(context);
                }
            }

            _encounteredContentTypes.Add(contentItem.ContentType);
            //_encounteredContentItems.Add(contentItem.ContentItemId);

            return context;
        }

        public async Task<IDescribeRelationshipsContext?> BuildRelationships(
            string contentItemId,
            IDescribeRelationshipsContext context)
        {
            ContentItem? contentItem = await context.ContentItemVersion.GetContentItem(_contentManager, contentItemId);
            if (contentItem == null)
            {
                //todoL: which excpetion
                throw new InvalidOperationException($"ContentItem with id {contentItemId} not found.");
            }

            //todo: overload () that accepts context (non root)
            // and version that accepts
            //todo: child context is same as parent. do we require all of these?
            // are they not used??
            return await BuildRelationships(
                contentItem,
                context.SourceNodeIdPropertyName,
                context.SourceNodeId,
                context.SourceNodeLabels,
                context.SyncNameProvider,
                context.ContentManager,
                context.ContentItemVersion,
                context,
                context.ServiceProvider,
                context.RootContentItem);
        }

        //todo: move any cypher generation into a query
        private static async Task<IEnumerable<ContentItemRelationship>> GetRelationships(
            IDescribeRelationshipsContext context,
            List<ContentItemRelationship> currentList,
            IDescribeRelationshipsContext parentContext)
        {
            foreach (var child in context.AvailableRelationships)
            {
                if (child == null)
                    continue;

                var parentRelationship = parentContext.AvailableRelationships.FirstOrDefault(x => x.Destination.All(child.Source.Contains));

                if (parentRelationship != null && !string.IsNullOrEmpty(parentRelationship.RelationshipPathString))
                {
                    var relationshipString = $"{parentRelationship.RelationshipPathString}-[r{context.CurrentDepth}:{child.Relationship}]-(d{context.CurrentDepth}:{string.Join(":", child.Destination!)})";
                    child.RelationshipPathString = relationshipString;
                }
                else
                {
                    child.RelationshipPathString = $@"match (s:{string.Join(":", context.SourceNodeLabels)} {{{context.SourceNodeIdPropertyName}: '{context.SourceNodeId}'}})-[r{1}:{child.Relationship}]-(d{1}:{string.Join(":", child.Destination!)})";
                }
            }

            currentList.AddRange(context.AvailableRelationships);

            foreach (var childContext in context.ChildContexts)
            {
                await GetRelationships((IDescribeRelationshipsContext)childContext, currentList, context);
            }

            return currentList;
        }
    }
}
