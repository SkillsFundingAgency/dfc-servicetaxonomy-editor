using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Content.Services.Interface;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Exceptions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Items;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Parts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.AllowSync;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Results.AllowSync;
using DFC.ServiceTaxonomy.GraphSync.Models;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MoreLinq;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    public class MergeGraphSyncer : IMergeGraphSyncer
    {
        private readonly IEnumerable<IContentItemGraphSyncer> _itemSyncers;
        private readonly IGraphSyncPartGraphSyncer _graphSyncPartGraphSyncer;
        private readonly ISyncNameProvider _syncNameProvider;
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

        private const string CreatedDatePropertyName = "CreatedDate";
        private const string ModifiedDatePropertyName = "ModifiedDate";

        //todo: commands only in context and create context using ActivatorUtilities.CreateInstance

        public MergeGraphSyncer(
            IEnumerable<IContentItemGraphSyncer> itemSyncers,
            IGraphSyncPartGraphSyncer graphSyncPartGraphSyncer,
            ISyncNameProvider syncNameProvider,
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
            _graphSyncPartGraphSyncer = graphSyncPartGraphSyncer;
            _syncNameProvider = syncNameProvider;
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
            _logger.LogDebug("SyncAllowed to {GraphReplicaSetName} for '{ContentItem}' {ContentType}?",
                graphReplicaSet.Name, contentItem.ToString(), contentItem.ContentType);

            // we use the existence of a GraphSync content part as a marker to indicate that the content item should be synced
            JObject? graphSyncPartContent = (JObject?)contentItem.Content[nameof(GraphSyncPart)];
            if (graphSyncPartContent == null)
                return AllowSyncResult.NotRequired;

            string? disableSyncContentItemVersionId = _memoryCache.Get<string>($"DisableSync_{contentItem.ContentItemVersionId}");
            if (disableSyncContentItemVersionId != null)
            {
                _logger.LogInformation("Not syncing {ContentType}:{ContentItemId}, version {ContentItemVersionId} as syncing has been disabled for it.",
                    contentItem.ContentType, contentItem.ContentItemId, disableSyncContentItemVersionId);
                return AllowSyncResult.NotRequired;
            }

            //todo: ContentType belongs in the context, either combine helper & context, or supply context to helper?
            _syncNameProvider.ContentType = contentItem.ContentType;

            _graphMergeContext = new GraphMergeContext(
                this, _syncNameProvider, graphReplicaSet, MergeNodeCommand, _replaceRelationshipsCommand,
                contentItem, contentManager, _contentItemVersionFactory, parentGraphMergeContext, _serviceProvider);

            await PopulateMergeNodeCommand(graphSyncPartContent);

            SetSourceNodeInReplaceRelationshipsCommand();

            //should it go in the context?
            _incomingPreviewContentPickerRelationships = await GetIncomingPreviewContentPickerRelationshipsWhenPublishing(
                graphReplicaSet,
                graphSyncPartContent,
                contentItem.ContentItemId);

            return await SyncAllowed();
        }

        private async Task PopulateMergeNodeCommand(JObject graphSyncPartContent)
        {
            MergeNodeCommand.NodeLabels.UnionWith(await _syncNameProvider.NodeLabels());
            MergeNodeCommand.IdPropertyName = _syncNameProvider.IdPropertyName();

            //todo: we could move population of the time properties to later when syncing, rather than at syncallowed time

            // add created and modified dates to all content items
            if (_graphMergeContext!.ContentItem.CreatedUtc.HasValue)
            {
                MergeNodeCommand.Properties.Add(await _syncNameProvider.PropertyName(CreatedDatePropertyName),
                    _graphMergeContext.ContentItem.CreatedUtc.Value);
            }

            if (_graphMergeContext.ContentItem.ModifiedUtc.HasValue)
            {
                MergeNodeCommand.Properties.Add(await _syncNameProvider.PropertyName(ModifiedDatePropertyName),
                    _graphMergeContext.ContentItem.ModifiedUtc.Value);
            }

            await _graphSyncPartGraphSyncer.AddSyncComponents(graphSyncPartContent, _graphMergeContext!);
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

        public async Task<IMergeNodeCommand?> SyncToGraphReplicaSet()
        {
            _logger.LogDebug("Syncing {ContentItem}.", _graphMergeContext?.ContentItem.ToString());

            await SyncEmbedded();

            _logger.LogInformation($"Syncing {_graphMergeContext!.ContentItem.ContentType} : {_graphMergeContext.ContentItem.ContentItemId} to {MergeNodeCommand}");
            await SyncComponentsToGraphReplicaSet(); //_graphMergeContext.GraphReplicaSet, recreateIncomingPreviewContentPickerRelationshipsCommands);

            return MergeNodeCommand;
        }

        public async Task<IMergeGraphSyncer?> SyncEmbedded(ContentItem contentItem)
        {
            _logger.LogDebug("Syncing embedded {ContentItem}.", contentItem.ToString());

            JObject? graphSyncPartContent = (JObject?)contentItem.Content[nameof(GraphSyncPart)];
            if (graphSyncPartContent == null)
                return null;

            var embeddedMergeContext = _graphMergeContext!.ChildContexts
                .Single(c => c.ContentItem.ContentItemId == contentItem.ContentItemId);

            _logger.LogDebug("Found existing GraphMergeContext for {ContentItem}.", contentItem.ToString());

            var embeddedMergeGraphSyncer = (MergeGraphSyncer)embeddedMergeContext.MergeGraphSyncer;

            if (!embeddedMergeGraphSyncer._syncNameProvider.GraphSyncPartSettings.PreexistingNode)
            {
                await ((MergeGraphSyncer)embeddedMergeContext.MergeGraphSyncer).SyncEmbedded();
            }

            return embeddedMergeGraphSyncer;
        }

        private async Task SyncEmbedded()
        {
            if (_graphMergeContext == null)
                throw new GraphSyncException($"You must call {nameof(SyncAllowed)} first.");

            await AddContentPartSyncComponents();

            //todo: bit hacky. best way to do this? remove this now?
            // work-around new taxonomy terms created with only DisplayText set
            if (!MergeNodeCommand.Properties.ContainsKey(_syncNameProvider.IdPropertyName())
                && MergeNodeCommand.Properties.ContainsKey(TitlePartGraphSyncer.NodeTitlePropertyName))
            {
                MergeNodeCommand.IdPropertyName = TitlePartGraphSyncer.NodeTitlePropertyName;
            }

            _graphMergeContext.ExtraCommands.AddRange(
                GetRecreateIncomingPreviewContentPickerRelationshipsCommands());
        }

        private IEnumerable<IReplaceRelationshipsCommand> GetRecreateIncomingPreviewContentPickerRelationshipsCommands()
        {
            //todo: need to support twoway, although not yet
            if (_incomingPreviewContentPickerRelationships?.Any() == true)
            {
                //todo: check relationship properties include any others that were already there

                return _incomingPreviewContentPickerRelationships
                    .Select(r => r.ToReplaceRelationshipsCommand(
                        _syncNameProvider,
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
            getDraftRelationshipsQuery.IdPropertyValue = _syncNameProvider.GetIdPropertyValue(
                graphSyncPartContent, _previewContentItemVersion);

            IEnumerable<INodeWithOutgoingRelationships?> incomingContentPickerRelationshipsOrDefault =
                await _graphCluster.Run(_previewContentItemVersion.GraphReplicaSetName, getDraftRelationshipsQuery);

            #pragma warning disable S1905 // Sonar needs updating to know about nullable references
            return incomingContentPickerRelationshipsOrDefault
                    .Where(n => n != null)
                    .Cast<INodeWithOutgoingRelationships>();
            #pragma warning restore S1905
        }

        private async Task AddContentPartSyncComponents()
        {
            foreach (IContentItemGraphSyncer itemSyncer in _itemSyncers)
            {
                //todo: allow syncers to chain or not?
                if (itemSyncer.CanSync(_graphMergeContext!.ContentItem))
                {
                    await itemSyncer.AddSyncComponents(_graphMergeContext);
                }
            }
        }

        //todo: should we add a AddIdSyncComponents method?

        private async Task SyncComponentsToGraphReplicaSet()
        {
            var commands = MoreEnumerable
                .TraverseBreadthFirst((IGraphMergeContext)_graphMergeContext!, ctx => ctx!.ChildContexts)
                .SelectMany(ctx =>
                {
                    var nodeCommands = new List<ICommand>();

                    if (ctx.ReplaceRelationshipsCommand.Relationships.Any())
                        nodeCommands.Add(ctx.ReplaceRelationshipsCommand);

                    if (ctx.ExtraCommands.Any())
                        nodeCommands.AddRange(ctx.ExtraCommands);

                    if (!ctx.SyncNameProvider.GraphSyncPartSettings.PreexistingNode)
                        nodeCommands.Add(ctx.MergeNodeCommand);

                    return nodeCommands;
                })
                .Reverse()
                .ToArray();

            await _graphMergeContext!.GraphReplicaSet.Run(commands);
        }

        private void SetSourceNodeInReplaceRelationshipsCommand()
        {
            _replaceRelationshipsCommand.SourceNodeLabels = new HashSet<string>(MergeNodeCommand.NodeLabels);
            _replaceRelationshipsCommand.SourceIdPropertyName = MergeNodeCommand.IdPropertyName;
            //todo: helper for this, used elsewhere
            _replaceRelationshipsCommand.SourceIdPropertyValue =
                _graphMergeContext!.MergeNodeCommand.Properties[_graphMergeContext.MergeNodeCommand.IdPropertyName!];
        }
    }
}
