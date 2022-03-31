﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.CosmosDb.Queries;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Exceptions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Items;
using DFC.ServiceTaxonomy.GraphSync.Helpers;
using DFC.ServiceTaxonomy.GraphSync.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Settings;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.CosmosDb
{
    public class CosmosDbDescribeContentItemHelper : IDescribeContentItemHelper
    {
        private readonly IContentManager _contentManager;
        private readonly IEnumerable<IContentItemGraphSyncer> _contentItemGraphSyncers;
        private readonly List<string> _encounteredContentTypes = new List<string>();
        private readonly IOptionsMonitor<GraphSyncSettings> _graphSyncSettings;

        public CosmosDbDescribeContentItemHelper(
            IContentManager contentManager,
            IEnumerable<IContentItemGraphSyncer> contentItemGraphSyncers,
            IOptionsMonitor<GraphSyncSettings> graphSyncSettings)
        {
            _contentItemGraphSyncers = contentItemGraphSyncers.OrderByDescending(s => s.Priority);

            _contentManager = contentManager;
            _graphSyncSettings = graphSyncSettings;
        }

        public async Task<IEnumerable<IQuery<object?>>> GetRelationshipCommands(IDescribeRelationshipsContext context)
        {
            var currentList = new List<ContentItemRelationship>();
            var allRelationships = await GetRelationships(context, currentList);

            var uniqueCommands = allRelationships
                .Select(relationship => relationship.RelationshipPathString)
                .GroupBy(relationshipPathString => relationshipPathString)
                .Select(relationshipPathStringGroup => relationshipPathStringGroup.First());

            return uniqueCommands
                .Select(c => new CosmosDbNodeAndNestedOutgoingRelationshipsQuery(c!)).Cast<IQuery<object?>>().ToList();
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
            IServiceProvider serviceProvider)
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
                contentManager, contentItemVersion, parentContext, serviceProvider);

            foreach (IContentItemGraphSyncer itemSyncer in _contentItemGraphSyncers)
            {
                //todo: allow syncers to chain or not? probably not
                if (itemSyncer.CanSync(context.ContentItem))
                {
                    await itemSyncer.AddRelationship(context);
                }
            }

            _encounteredContentTypes.Add(contentItem.ContentType);

            return context;
        }

        public async Task<IDescribeRelationshipsContext?> BuildRelationships(
            string contentItemId,
            IDescribeRelationshipsContext context)
        {
            ContentItem? contentItem = await context.ContentItemVersion.GetContentItem(_contentManager, contentItemId);
            if (contentItem == null)
            {
                throw new GraphSyncException($"ContentItem with id {contentItemId} not found.");
            }

            //todo: overload () that accepts context (non root)?
            //todo: child context is same as parent. do we require all of these?
            return await BuildRelationships(
                contentItem,
                context.SourceNodeIdPropertyName,
                context.SourceNodeId,
                context.SourceNodeLabels,
                context.SyncNameProvider,
                context.ContentManager,
                context.ContentItemVersion,
                context,
                context.ServiceProvider);
        }

        //todo: move any cypher generation into a query
        private static async Task<IEnumerable<ContentItemRelationship>> GetRelationships(
            IDescribeRelationshipsContext context,
            List<ContentItemRelationship> currentList)
        {
            foreach (var child in context.AvailableRelationships)
            {
                (_, var id) = DocumentHelper.GetContentTypeAndId(context.SourceNodeId);
                child.RelationshipPathString = $"select * from c where c.id = '{id}'|{context.SourceNodeLabels.FirstOrDefault()}";
            }

            currentList.AddRange(context.AvailableRelationships);

            foreach (var childContext in context.ChildContexts)
            {
                await GetRelationships((IDescribeRelationshipsContext)childContext, currentList);
            }

            return currentList;
        }
    }
}
