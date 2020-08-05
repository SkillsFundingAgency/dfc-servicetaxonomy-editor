using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Exceptions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    //todo: have to refactor sync. currently with bags, a single sync will occur in multiple transactions
    // so if a validation fails for example, the graph will be left in an incomplete synced state
    // need to gather up all commands, then execute them in a single transaction
    // giving the commands the opportunity to validate the results before the transaction is committed
    // so any validation failure rolls back the whole sync operation
    public class MergeGraphSyncer : IMergeGraphSyncer
    {
        private readonly IEnumerable<IContentItemGraphSyncer> _itemSyncers;
        private readonly IGraphSyncHelper _graphSyncHelper;
        private readonly IReplaceRelationshipsCommand _replaceRelationshipsCommand;
        private readonly IMemoryCache _memoryCache;
        private readonly IContentItemVersionFactory _contentItemVersionFactory;
        private readonly INeutralContentItemVersion _neutralContentItemVersion;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MergeGraphSyncer> _logger;

        //todo: tidy these up? make more public??
        public IMergeNodeCommand MergeNodeCommand { get; }
        private GraphMergeContext? _graphMergeContext;
        public IGraphMergeContext? GraphMergeContext => _graphMergeContext;
        private ContentItem? _contentItem;
        private List<INodeWithOutgoingRelationships>? _incomingGhostRelationships;

        public MergeGraphSyncer(
            IEnumerable<IContentItemGraphSyncer> itemSyncers,
            IGraphSyncHelper graphSyncHelper,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            IMemoryCache memoryCache,
            IContentItemVersionFactory contentItemVersionFactory,
            INeutralContentItemVersion neutralContentItemVersion,
            IServiceProvider serviceProvider,
            ILogger<MergeGraphSyncer> logger)
        {
            _itemSyncers = itemSyncers.OrderByDescending(s => s.Priority);
            _graphSyncHelper = graphSyncHelper;
            MergeNodeCommand = mergeNodeCommand;
            _replaceRelationshipsCommand = replaceRelationshipsCommand;
            _memoryCache = memoryCache;
            _contentItemVersionFactory = contentItemVersionFactory;
            _neutralContentItemVersion = neutralContentItemVersion;
            _serviceProvider = serviceProvider;
            _logger = logger;

            _graphMergeContext = null;
            _incomingGhostRelationships = null;
        }

        public async Task<IAllowSyncResult> SyncToGraphReplicaSetIfAllowed(
            IGraphReplicaSet graphReplicaSet,
            ContentItem contentItem,
            IContentManager contentManager,
            IGraphMergeContext? parentGraphMergeContext = null)
        {
            IAllowSyncResult allowSyncResult = await SyncAllowed(graphReplicaSet, contentItem, contentManager, parentGraphMergeContext);

            if (allowSyncResult.AllowSync == SyncStatus.Allowed)
                await SyncToGraphReplicaSet();

            return allowSyncResult;
        }

        public async Task<IAllowSyncResult> SyncAllowed(
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
                return AllowSyncResult.NotRequired;

            string? disableSyncContentItemVersionId = _memoryCache.Get<string>($"DisableSync_{contentItem.ContentItemVersionId}");
            if (disableSyncContentItemVersionId != null)
            {
                _logger.LogInformation($"Not syncing {contentItem.ContentType}:{contentItem.ContentItemId}, version {disableSyncContentItemVersionId} as syncing has been disabled for it");
                return AllowSyncResult.NotRequired;
            }

            //todo: add log to actual sync
            _logger.LogDebug($"Checking if sync allowed for {contentItem.ContentType} : {contentItem.ContentItemId}");

            //todo: ContentType belongs in the context, either combine helper & context, or supply context to helper?
            _graphSyncHelper.ContentType = contentItem.ContentType;

            MergeNodeCommand.NodeLabels.UnionWith(await _graphSyncHelper.NodeLabels());
            MergeNodeCommand.IdPropertyName = _graphSyncHelper.IdPropertyName();

            //Add created and modified dates to all content items
            //todo: store as neo's DateTime? especially if api doesn't match the string format
            if (contentItem.CreatedUtc.HasValue)
                MergeNodeCommand.Properties.Add(await _graphSyncHelper.PropertyName("CreatedDate"), contentItem.CreatedUtc.Value);

            if (contentItem.ModifiedUtc.HasValue)
                MergeNodeCommand.Properties.Add(await _graphSyncHelper.PropertyName("ModifiedDate"), contentItem.ModifiedUtc.Value);

            SetSourceNodeInReplaceRelationshipsCommand(graphReplicaSet, graphSyncPartContent);

            _graphMergeContext = new GraphMergeContext(
                _graphSyncHelper, graphReplicaSet, MergeNodeCommand, _replaceRelationshipsCommand,
                contentItem, contentManager, _contentItemVersionFactory, parentGraphMergeContext);

            //should it go in the context?
            _incomingGhostRelationships = await GetIncomingGhostRelationshipsWhenPublishing(
                graphReplicaSet,
                graphSyncPartContent);

            // move into context?
            _contentItem = contentItem;

            return await SyncAllowed(contentItem);
        }

        private async Task<IAllowSyncResult> SyncAllowed(ContentItem contentItem)
        {
            IAllowSyncResult syncAllowedResult = new AllowSyncResult();

            foreach (IContentItemGraphSyncer itemSyncer in _itemSyncers)
            {
                //todo: allow syncers to chain or not?
                if (itemSyncer.CanSync(contentItem))
                {
                    await itemSyncer.AllowSync(_graphMergeContext!, syncAllowedResult);
                }
            }

            return syncAllowedResult;
        }

        //todo: need to split generating commands, and syncing
        public async Task<IMergeNodeCommand?> SyncToGraphReplicaSet()
        {
            if (_graphMergeContext == null)
                throw new GraphSyncException($"You must call {nameof(SyncAllowed)} before calling {nameof(SyncToGraphReplicaSet)}");

            await AddContentPartSyncComponents(_contentItem!);

            //todo: bit hacky. best way to do this? remove this now?
            // work-around new taxonomy terms created with only DisplayText set
            if (!MergeNodeCommand.Properties.ContainsKey(_graphSyncHelper.IdPropertyName())
                && MergeNodeCommand.Properties.ContainsKey(TitlePartGraphSyncer.NodeTitlePropertyName))
            {
                MergeNodeCommand.IdPropertyName = TitlePartGraphSyncer.NodeTitlePropertyName;
            }

            IEnumerable<IReplaceRelationshipsCommand> recreateGhostRelationshipsCommands =
                GetRecreateGhostRelationshipCommands();

            //todo: get delete ghost command: no need for them now relationships have been stored?

            _logger.LogInformation($"Syncing {_contentItem!.ContentType} : {_contentItem.ContentItemId} to {MergeNodeCommand}");
            await SyncComponentsToGraphReplicaSet(_graphMergeContext.GraphReplicaSet, recreateGhostRelationshipsCommands);

            return MergeNodeCommand;
        }

        private IEnumerable<IReplaceRelationshipsCommand> GetRecreateGhostRelationshipCommands()
        {
            //todo: need to support twoway
            if (_incomingGhostRelationships?.Any() == true)
            {
                return _incomingGhostRelationships
                    .Select(r => r.ToReplaceRelationshipsCommand(_graphSyncHelper));
            }

            return Enumerable.Empty<IReplaceRelationshipsCommand>();
        }

        //todo: return null?
        private async Task<List<INodeWithOutgoingRelationships>> GetIncomingGhostRelationshipsWhenPublishing(
            IGraphReplicaSet graphReplicaSet,
            dynamic graphSyncPartContent)
        {
            if (graphReplicaSet.Name != GraphReplicaSetNames.Published)
                return new List<INodeWithOutgoingRelationships>();

            IGetDraftRelationships getDraftRelationships = _serviceProvider.GetRequiredService<IGetDraftRelationships>();

            getDraftRelationships.ContentType = _contentItem!.ContentType;
            getDraftRelationships.IdPropertyValue = _graphSyncHelper.GetIdPropertyValue(
                graphSyncPartContent, _neutralContentItemVersion);

            //todo: does it need to be ?
            List<INodeWithOutgoingRelationships?> incomingGhostRelationships = await graphReplicaSet.Run(getDraftRelationships);

            //todo: return IEnumerable??

            #pragma warning disable S1905 // Sonar needs updating to know about nullable references
            return incomingGhostRelationships
                .Where(n => n != null)
                .Cast<INodeWithOutgoingRelationships>()
                .ToList();
            #pragma warning restore S1905
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

        //todo: should we add a AddIdSyncComponents method?

        private async Task SyncComponentsToGraphReplicaSet(
            IGraphReplicaSet graphReplicaSet,
            IEnumerable<ICommand> extraCommands)
        {
            List<ICommand> commands = new List<ICommand>();

            if (!_graphSyncHelper.GraphSyncPartSettings.PreexistingNode)
                commands.Add(MergeNodeCommand);

            if (extraCommands.Any())
                commands.AddRange(extraCommands);

            if (_replaceRelationshipsCommand.Relationships.Any())
                commands.Add(_replaceRelationshipsCommand);

            await graphReplicaSet.Run(commands.ToArray());
        }

        private void SetSourceNodeInReplaceRelationshipsCommand(IGraphReplicaSet graphReplicaSet, dynamic graphSyncPartContent)
        {
            _replaceRelationshipsCommand.SourceNodeLabels = new HashSet<string>(MergeNodeCommand.NodeLabels);
            _replaceRelationshipsCommand.SourceIdPropertyName = MergeNodeCommand.IdPropertyName;
            _replaceRelationshipsCommand.SourceIdPropertyValue = _graphSyncHelper.GetIdPropertyValue(
                graphSyncPartContent, _contentItemVersionFactory.Get(graphReplicaSet.Name));
        }
    }
}
