using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.CosmosDb.Queries;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Contexts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Exceptions;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Items;
using DFC.ServiceTaxonomy.DataSync.Helpers;
using DFC.ServiceTaxonomy.DataSync.Interfaces;
using DFC.ServiceTaxonomy.DataSync.Settings;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.DataSync.CosmosDb
{
    public class CosmosDbDescribeContentItemHelper : IDescribeContentItemHelper
    {
        private readonly IContentManager _contentManager;
        private readonly IEnumerable<IContentItemDataSyncer> _contentItemDataSyncers;
        private readonly List<string> _encounteredContentTypes = new List<string>();
        private readonly IOptionsMonitor<GraphSyncSettings> _dataSyncSettings;

        public CosmosDbDescribeContentItemHelper(
            IContentManager contentManager,
            IEnumerable<IContentItemDataSyncer> contentItemDataSyncers,
            IOptionsMonitor<GraphSyncSettings> dataSyncSettings)
        {
            _contentItemDataSyncers = contentItemDataSyncers.OrderByDescending(s => s.Priority);

            _contentManager = contentManager;
            _dataSyncSettings = dataSyncSettings;
        }

        public async Task<IEnumerable<IQuery<object?>>> GetRelationshipCommands(IDescribeRelationshipsContext context)
        {
            var currentList = new List<(string id, string contentType)>();
            var allRelationships = (await GetRelationships(context, currentList)).ToList();

            if (!allRelationships.Any())
            {
                (_, var id) = DocumentHelper.GetContentTypeAndId(context.SourceNodeId);
                var type = context.SourceNodeLabels.First(snl => !snl.Equals("Resource"));
                allRelationships.Add((id.ToString(), type));
            }

            var uniqueCommands = allRelationships.Distinct();

            return uniqueCommands
                .Select(c => new CosmosDbNodeAndNestedOutgoingRelationshipsQuery("SELECT * FROM c WHERE c.id = @id0", "@id0", c.id, c.contentType)).Cast<IQuery<object?>>().ToList();
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
            var dataSyncPartSettings = syncNameProvider.GetDataSyncPartSettings(contentItem.ContentType);

            int maxDepthFromHere;

            if (parentContext == null)
            {
                maxDepthFromHere = Math.Min(dataSyncPartSettings.VisualiserNodeDepth ?? int.MaxValue,
                    //todo: store in root in case changes mid flow?
                    _dataSyncSettings.CurrentValue.MaxVisualiserNodeDepth);
            }
            else
            {
                if (_encounteredContentTypes.Any(x => x == contentItem.ContentType))
                    return null;

                maxDepthFromHere = Math.Min(parentContext.MaxDepthFromHere - 1,
                    dataSyncPartSettings.VisualiserNodeDepth ?? int.MaxValue);
            }

            if (maxDepthFromHere <= 0)
                return null;

            var context = new DescribeRelationshipsContext(
                sourceNodeIdPropertyName, sourceNodeId, sourceNodeLabels, contentItem, maxDepthFromHere, syncNameProvider,
                contentManager, contentItemVersion, parentContext, serviceProvider);

            foreach (IContentItemDataSyncer itemSyncer in _contentItemDataSyncers)
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
                throw new DataSyncException($"ContentItem with id {contentItemId} not found.");
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
        private static async Task<IEnumerable<(string id, string contentType)>> GetRelationships(IDescribeRelationshipsContext context, List<(string id, string contentType)> currentList)
        {
            foreach (var child in context.AvailableRelationships)
            {
                (_, var id) = DocumentHelper.GetContentTypeAndId(context.SourceNodeId);
                var contentType = context.SourceNodeLabels.First(snl => !snl.Equals("Resource"));
                currentList.Add((id.ToString(), contentType));
            }
            foreach (var childContext in context.ChildContexts)
            {
                await GetRelationships((IDescribeRelationshipsContext)childContext, currentList);
            }
            return currentList;
        }
    }
}
