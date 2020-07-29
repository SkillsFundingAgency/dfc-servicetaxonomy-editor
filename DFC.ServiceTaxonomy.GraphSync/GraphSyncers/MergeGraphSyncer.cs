using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Exceptions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    public enum SyncStatus
    {
        /// <summary>
        /// Syncing is not required, because either the ContentItem doesn't have a GraphSyncPart,
        /// or syncing has been temporarily disabled for the content item.
        /// </summary>
        NotRequired,
        /// <summary>
        /// All components have given the go ahead.
        /// </summary>
        Allowed,
        /// <summary>
        /// A component has blocked the sync.
        /// As neo4j doesn't support 2 phase commit, or write transactions across graphs (even in fabric),
        /// we allow components to block a sync before we attempt to sync.
        /// This allows us to keep OC's db, and the published and preview graphs consistent
        /// (even though there is a small window to break this).
        /// </summary>
        Blocked
    }

    //todo: have to refactor sync. currently with bags, a single sync will occur in multiple transactions
    // so if a validation fails for example, the graph will be left in an incomplete synced state
    // need to gather up all commands, then execute them in a single transaction
    // giving the commands the opportunity to validate the results before the transaction is committed
    // so any validation failure rolls back the whole sync operation
    public class MergeGraphSyncer : IMergeGraphSyncer
    {
        private readonly IEnumerable<IContentItemGraphSyncer> _itemSyncers;
        private readonly IGraphSyncHelper _graphSyncHelper;
        private readonly IMergeNodeCommand _mergeNodeCommand;
        private readonly IReplaceRelationshipsCommand _replaceRelationshipsCommand;
        private readonly IMemoryCache _memoryCache;
        private readonly IContentItemVersionFactory _contentItemVersionFactory;
        private readonly ILogger<MergeGraphSyncer> _logger;

        private GraphMergeContext? _graphMergeContext;
        private ContentItem? _contentItem;

        public MergeGraphSyncer(
            IEnumerable<IContentItemGraphSyncer> itemSyncers,
            IGraphSyncHelper graphSyncHelper,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            IMemoryCache memoryCache,
            IContentItemVersionFactory contentItemVersionFactory,
            ILogger<MergeGraphSyncer> logger)
        {
            _itemSyncers = itemSyncers.OrderByDescending(s => s.Priority);
            _graphSyncHelper = graphSyncHelper;
            _mergeNodeCommand = mergeNodeCommand;
            _replaceRelationshipsCommand = replaceRelationshipsCommand;
            _memoryCache = memoryCache;
            _contentItemVersionFactory = contentItemVersionFactory;
            _logger = logger;

            _graphMergeContext = null;
        }

        public async Task<SyncStatus> SyncToGraphReplicaSetIfAllowed(
            IGraphReplicaSet graphReplicaSet,
            ContentItem contentItem,
            IContentManager contentManager,
            IGraphMergeContext? parentGraphMergeContext = null)
        {
            SyncStatus syncStatus = await SyncAllowed(graphReplicaSet, contentItem, contentManager, parentGraphMergeContext);

            if (syncStatus == SyncStatus.Allowed)
                await SyncToGraphReplicaSet();

            return syncStatus;
        }

        public async Task<SyncStatus> SyncAllowed(
            IGraphReplicaSet graphReplicaSet,
            ContentItem contentItem,
            IContentManager contentManager,
            IGraphMergeContext? parentGraphMergeContext = null)
        {
            // we use the existence of a GraphSync content part as a marker to indicate that the content item should be synced
            // so we silently noop if it's not present
            JObject? graphSyncPartContent = (JObject?)contentItem.Content[nameof(GraphSyncPart)];
            //todo: text -> id?
            //todo: why graph sync has tags in features, others don't?
            if (graphSyncPartContent == null)
                return SyncStatus.NotRequired;

            string? disableSyncContentItemVersionId = _memoryCache.Get<string>($"DisableSync_{contentItem.ContentItemVersionId}");
            if (disableSyncContentItemVersionId != null)
            {
                _logger.LogInformation($"Not syncing {contentItem.ContentType}:{contentItem.ContentItemId}, version {disableSyncContentItemVersionId} as syncing has been disabled for it");
                return SyncStatus.NotRequired;
            }

            _logger.LogDebug($"Syncing {contentItem.ContentType} : {contentItem.ContentItemId}");

            //todo: ContentType belongs in the context, either combine helper & context, or supply context to helper?
            _graphSyncHelper.ContentType = contentItem.ContentType;

            _mergeNodeCommand.NodeLabels.UnionWith(await _graphSyncHelper.NodeLabels());
            _mergeNodeCommand.IdPropertyName = _graphSyncHelper.IdPropertyName();

            //Add created and modified dates to all content items
            //todo: store as neo's DateTime? especially if api doesn't match the string format
            if (contentItem.CreatedUtc.HasValue)
                _mergeNodeCommand.Properties.Add(await _graphSyncHelper.PropertyName("CreatedDate"), contentItem.CreatedUtc.Value);

            if (contentItem.ModifiedUtc.HasValue)
                _mergeNodeCommand.Properties.Add(await _graphSyncHelper.PropertyName("ModifiedDate"), contentItem.ModifiedUtc.Value);

            SetSourceNodeInReplaceRelationshipsCommand(graphReplicaSet, graphSyncPartContent);

            _graphMergeContext = new GraphMergeContext(
                _graphSyncHelper, graphReplicaSet, _mergeNodeCommand, _replaceRelationshipsCommand,
                contentItem, contentManager, _contentItemVersionFactory, parentGraphMergeContext);

            // move into context?
            _contentItem = contentItem;

            return SyncStatus.Allowed;
        }

        public async Task<IMergeNodeCommand?> SyncToGraphReplicaSet()
        {
            if (_graphMergeContext == null)
                throw new GraphSyncException($"You must call {nameof(SyncAllowed)} before calling {nameof(SyncToGraphReplicaSet)}");

            await AddContentPartSyncComponents(_contentItem!);

            //todo: bit hacky. best way to do this?
            // work-around new taxonomy terms created with only DisplayText set
            if (!_mergeNodeCommand.Properties.ContainsKey(_graphSyncHelper.IdPropertyName())
                && _mergeNodeCommand.Properties.ContainsKey(TitlePartGraphSyncer.NodeTitlePropertyName))
            {
                _mergeNodeCommand.IdPropertyName = TitlePartGraphSyncer.NodeTitlePropertyName;
            }

            _logger.LogInformation($"Syncing {_contentItem!.ContentType} : {_contentItem.ContentItemId} to {_mergeNodeCommand}");
            await SyncComponentsToGraphReplicaSet(_graphMergeContext.GraphReplicaSet);

            return _mergeNodeCommand;
        }

        private async Task AddContentPartSyncComponents(ContentItem contentItem)
        {
            foreach (IContentItemGraphSyncer itemSyncer in _itemSyncers)
            {
                //todo: allow syncers to chain or not?
                if (itemSyncer.CanSync(contentItem))
                {
                    await itemSyncer.AddSyncComponents(_graphMergeContext!);
                }
            }
        }

        private async Task SyncComponentsToGraphReplicaSet(IGraphReplicaSet graphReplicaSet)
        {
            List<ICommand> commands = new List<ICommand>();

            if (!_graphSyncHelper.GraphSyncPartSettings.PreexistingNode)
                commands.Add(_mergeNodeCommand);

            if (_replaceRelationshipsCommand.Relationships.Any())
                commands.Add(_replaceRelationshipsCommand);

            await graphReplicaSet.Run(commands.ToArray());
        }

        private void SetSourceNodeInReplaceRelationshipsCommand(IGraphReplicaSet graphReplicaSet, dynamic graphSyncPartContent)
        {
            _replaceRelationshipsCommand.SourceNodeLabels = new HashSet<string>(_mergeNodeCommand.NodeLabels);
            _replaceRelationshipsCommand.SourceIdPropertyName = _mergeNodeCommand.IdPropertyName;
            _replaceRelationshipsCommand.SourceIdPropertyValue = _graphSyncHelper.GetIdPropertyValue(
                graphSyncPartContent, _contentItemVersionFactory.Get(graphReplicaSet.Name));
        }
    }
}
