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
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Helpers;
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
        private readonly IOptions<GraphSyncSettings> _graphSyncSettings;

        public DescribeContentItemHelper(
            IContentManager contentManager,
            IEnumerable<IContentItemGraphSyncer> contentItemGraphSyncers,
            IOptions<GraphSyncSettings> graphSyncSettings)
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

            //todo: need to apply max node depth for child items, not just at the top level
            var graphSyncPartSettings = context.SyncNameProvider.GetGraphSyncPartSettings(context.ContentItem.ContentType);
            int maxVisualiserDepth = graphSyncPartSettings.VisualiserNodeDepth != null
                ? Math.Min(graphSyncPartSettings.VisualiserNodeDepth.Value,
                    _graphSyncSettings.Value.MaxVisualiserNodeDepth)
                : _graphSyncSettings.Value.MaxVisualiserNodeDepth;

//            var allRelationships = await ContentItemRelationshipToCypherHelper.GetRelationships(context, currentList, parentContext, maxVisualiserDepth);
            var allRelationships = await ContentItemRelationshipToCypherHelper.GetRelationships(context, currentList, context, maxVisualiserDepth);
            var uniqueCommands = allRelationships.Select(z => z.RelationshipPathString).GroupBy(x => x).Select(g => g.First());

            List<IQuery<object?>> commandsToReturn = uniqueCommands
                .Select(c => new NodeAndNestedOutgoingRelationshipsQuery(c!)).Cast<IQuery<object?>>().ToList();

            //todo: for occupation and skill, we need to filter out nodes that have just the skos__Concept and Resource labels (and others)
            // but allow other nodes that have a skos__Concept label, such as occupations and skills
            // (or filter on relationships, whitelist whatever)
            //todo: add a setting to graphsyncsettings for the filtering (for now we'll set incoming to 0 for occs & skills)
            commandsToReturn.Add(new SubgraphQuery(
                context.SourceNodeLabels,
                context.SourceNodeIdPropertyName,
                context.SourceNodeId,
                SubgraphQuery.RelationshipFilterIncoming,
                graphSyncPartSettings.VisualiserIncomingRelationshipsPathLength ?? 1));

            return commandsToReturn;
        }

        //todo: contentmanager
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
                    5); //todo: appsettings.VisualiserNodeDepthMax);
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

        // public async Task BuildRelationships(string contentItemId, IDescribeRelationshipsContext context)
        // {
        //     //todo: check for null
        //     ContentItem? contentItem = await context.ContentItemVersion.GetContentItem(_contentManager, contentItemId);
        //     if (contentItem == null)
        //     {
        //         //todoL: which excpetion
        //         throw new InvalidOperationException($"ContentItem with id {contentItemId} not found.");
        //     }
        //
        //     // if (_encounteredContentItems.Any(x => x == contentItem.ContentItemId)
        //     //     || _encounteredContentTypes.Any(x => x == contentItem.ContentType))
        //     if (_encounteredContentTypes.Any(x => x == contentItem.ContentType))
        //     {
        //         return;
        //     }
        //
        //     //todo: can we just store parentcontext and contentitem?
        //     var childContext = new DescribeRelationshipsContext(
        //         context.SourceNodeIdPropertyName,
        //         context.SourceNodeId,
        //         context.SourceNodeLabels,
        //         contentItem!,
        //         context.SyncNameProvider,
        //         context.ContentManager,
        //         context.ContentItemVersion,
        //         context,
        //         context.ServiceProvider,
        //         context.RootContentItem);
        //
        //     await BuildRelationships(contentItem!, childContext);
        // }

        // public async Task BuildRelationships(ContentItem contentItem, IDescribeRelationshipsItemSyncContext context)
        // {
        //     //todo: only 2nd part required?
        //     //todo: don't create context if excluding
        //     if (_encounteredContentItems.Any(x => x == contentItem.ContentItemId) || _encounteredContentTypes.Any(x => x == contentItem.ContentType))
        //     {
        //         return;
        //     }
        //
        //     foreach (IContentItemGraphSyncer itemSyncer in _contentItemGraphSyncers)
        //     {
        //         //todo: allow syncers to chain or not? probably not
        //         if (itemSyncer.CanSync(context.ContentItem))
        //         {
        //             await itemSyncer.AddRelationship(context);
        //         }
        //     }
        //
        //     _encounteredContentTypes.Add(contentItem.ContentType);
        //     _encounteredContentItems.Add(contentItem.ContentItemId);
        // }
    }
}
