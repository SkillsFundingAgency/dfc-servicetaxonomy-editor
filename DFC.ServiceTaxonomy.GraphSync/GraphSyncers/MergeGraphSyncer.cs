using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Exceptions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Items;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.AllowSync;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Results.AllowSync;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    public class MergeGraphSyncer : IMergeGraphSyncer
    {
        private readonly IEnumerable<IContentItemGraphSyncer> _itemSyncers;
        private readonly IGraphSyncHelper _graphSyncHelper;
        private readonly IReplaceRelationshipsCommand _replaceRelationshipsCommand;
        private readonly IMemoryCache _memoryCache;
        private readonly IContentItemVersionFactory _contentItemVersionFactory;
        private readonly IPublishedContentItemVersion _publishedContentItemVersion;
        private readonly IPreviewContentItemVersion _previewContentItemVersion;
        private readonly IServiceProvider _serviceProvider;
        private readonly IGraphCluster _graphCluster;
        private readonly IContentItemsService _contentItemsService;
        private readonly ILogger<MergeGraphSyncer> _logger;

        //todo: tidy these up? make more public??
        public IMergeNodeCommand MergeNodeCommand { get; }
        private GraphMergeContext? _graphMergeContext;
        public IGraphMergeContext? GraphMergeContext => _graphMergeContext;
        private IEnumerable<INodeWithOutgoingRelationships>? _incomingPreviewContentPickerRelationships;

        public MergeGraphSyncer(
            IEnumerable<IContentItemGraphSyncer> itemSyncers,
            IGraphSyncHelper graphSyncHelper,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            IMemoryCache memoryCache,
            IContentItemVersionFactory contentItemVersionFactory,
            IPublishedContentItemVersion publishedContentItemVersion,
            IPreviewContentItemVersion previewContentItemVersion,
            IServiceProvider serviceProvider,
            IGraphCluster graphCluster,
            IContentItemsService contentItemsService,
            ILogger<MergeGraphSyncer> logger)
        {
            _itemSyncers = itemSyncers.OrderByDescending(s => s.Priority);
            _graphSyncHelper = graphSyncHelper;
            MergeNodeCommand = mergeNodeCommand;
            _replaceRelationshipsCommand = replaceRelationshipsCommand;
            _memoryCache = memoryCache;
            _contentItemVersionFactory = contentItemVersionFactory;
            _publishedContentItemVersion = publishedContentItemVersion;
            _previewContentItemVersion = previewContentItemVersion;
            _serviceProvider = serviceProvider;
            _graphCluster = graphCluster;
            _contentItemsService = contentItemsService;
            _logger = logger;

            _graphMergeContext = null;
            _incomingPreviewContentPickerRelationships = null;
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
            _incomingPreviewContentPickerRelationships = await GetIncomingPreviewContentPickerRelationshipsWhenPublishing(
                graphReplicaSet,
                graphSyncPartContent,
                contentItem.ContentItemId);

            return await SyncAllowed();
        }

        private async Task<IAllowSyncResult> SyncAllowed()
        {
            IAllowSyncResult syncAllowedResult = new AllowSyncResult();

            foreach (IContentItemGraphSyncer itemSyncer in _itemSyncers)
            {
                //todo: allow syncers to chain or not? probably not
                if (itemSyncer.CanSync(_graphMergeContext!.ContentItem))
                {
                    await itemSyncer.AllowSync(_graphMergeContext, syncAllowedResult);
                }
            }

            return syncAllowedResult;
        }

        //todo: need to split generating commands, and syncing, to make sync to a graph atomic
        public async Task<IMergeNodeCommand?> SyncToGraphReplicaSet()
        {
            if (_graphMergeContext == null)
                throw new GraphSyncException($"You must call {nameof(SyncAllowed)} before calling {nameof(SyncToGraphReplicaSet)}.");

            await AddContentPartSyncComponents(_graphMergeContext.ContentItem);

            //todo: bit hacky. best way to do this? remove this now?
            // work-around new taxonomy terms created with only DisplayText set
            if (!MergeNodeCommand.Properties.ContainsKey(_graphSyncHelper.IdPropertyName())
                && MergeNodeCommand.Properties.ContainsKey(TitlePartGraphSyncer.NodeTitlePropertyName))
            {
                MergeNodeCommand.IdPropertyName = TitlePartGraphSyncer.NodeTitlePropertyName;
            }

            IEnumerable<IReplaceRelationshipsCommand> recreateIncomingPreviewContentPickerRelationshipsCommands =
                GetRecreateIncomingPreviewContentPickerRelationshipsCommands();

            _logger.LogInformation($"Syncing {_graphMergeContext.ContentItem.ContentType} : {_graphMergeContext.ContentItem.ContentItemId} to {MergeNodeCommand}");
            await SyncComponentsToGraphReplicaSet(_graphMergeContext.GraphReplicaSet, recreateIncomingPreviewContentPickerRelationshipsCommands);

            return MergeNodeCommand;
        }

        private IEnumerable<IReplaceRelationshipsCommand> GetRecreateIncomingPreviewContentPickerRelationshipsCommands()
        {
            //todo: need to support twoway, although not yet
            if (_incomingPreviewContentPickerRelationships?.Any() == true)
            {
                //todo: check relationship properties include any others that were already there

                return _incomingPreviewContentPickerRelationships
                    .Select(r => r.ToReplaceRelationshipsCommand(
                        _graphSyncHelper,
                        _previewContentItemVersion,
                        _publishedContentItemVersion,
                        false));
            }

            return Enumerable.Empty<IReplaceRelationshipsCommand>();
        }

        //todo: don't want to do this for embedded items
        private async Task<IEnumerable<INodeWithOutgoingRelationships>> GetIncomingPreviewContentPickerRelationshipsWhenPublishing(
            IGraphReplicaSet graphReplicaSet,
            dynamic graphSyncPartContent,
            string contentItemId)
        {
            // we only need to recreate incoming relationships
            // if we're publishing and there isn't currently a published version
            if (graphReplicaSet.Name != GraphReplicaSetNames.Published
                || await _contentItemsService.HasExistingPublishedVersion(contentItemId))
            {
                return Enumerable.Empty<INodeWithOutgoingRelationships>();
            }

            // allow sync is called concurrently for preview and published
            // so we could get the before or after incoming relationships
            // either should do, but perhaps we should do it serially to consistently fetch the _before_ incoming relationships?
            IGetIncomingContentPickerRelationshipsQuery getDraftRelationshipsQuery =
                _serviceProvider.GetRequiredService<IGetIncomingContentPickerRelationshipsQuery>();

            getDraftRelationshipsQuery.NodeLabels = MergeNodeCommand.NodeLabels;
            getDraftRelationshipsQuery.IdPropertyName = MergeNodeCommand.IdPropertyName;
            getDraftRelationshipsQuery.IdPropertyValue = _graphSyncHelper.GetIdPropertyValue(
                graphSyncPartContent, _previewContentItemVersion);

            IEnumerable<INodeWithOutgoingRelationships?> incomingContentPickerRelationshipsOrDefault =
                await _graphCluster.Run(_previewContentItemVersion.GraphReplicaSetName, getDraftRelationshipsQuery);

            #pragma warning disable S1905 // Sonar needs updating to know about nullable references
            return incomingContentPickerRelationshipsOrDefault
                    .Where(n => n != null)
                    .Cast<INodeWithOutgoingRelationships>();
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
