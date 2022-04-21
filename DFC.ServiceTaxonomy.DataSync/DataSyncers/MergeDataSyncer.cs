using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Content.Services.Interface;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Contexts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Exceptions;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Helpers;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Items;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Parts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Results.AllowSync;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Parts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Results.AllowSync;
using DFC.ServiceTaxonomy.DataSync.Interfaces;
using DFC.ServiceTaxonomy.DataSync.Interfaces.Queries;
using DFC.ServiceTaxonomy.DataSync.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MoreLinq;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers
{

    public class MergeDataSyncer : IMergeDataSyncer
    {
        private readonly IEnumerable<IContentItemDataSyncer> _itemSyncers;
        private readonly IDataSyncPartDataSyncer _dataSyncPartDataSyncer;
        private readonly ISyncNameProvider _syncNameProvider;
        private readonly IReplaceRelationshipsCommand _replaceRelationshipsCommand;
        private readonly IContentItemVersionFactory _contentItemVersionFactory;
        private readonly IPublishedContentItemVersion _publishedContentItemVersion;
        private readonly IPreviewContentItemVersion _previewContentItemVersion;
        private readonly IServiceProvider _serviceProvider;
        private readonly IDataSyncCluster _dataSyncCluster;
        private readonly IContentItemsService _contentItemsService;
        private readonly ILogger<MergeDataSyncer> _logger;

        //todo: tidy these up? make more public??
        public IMergeNodeCommand MergeNodeCommand { get; }
        private DataMergeContext? _dataSyncMergeContext;
        public IDataMergeContext? DataSyncMergeContext => _dataSyncMergeContext;
        private IEnumerable<INodeWithOutgoingRelationships>? _incomingPreviewContentPickerRelationships;

        private const string CreatedDatePropertyName = "CreatedDate";
        private const string ModifiedDatePropertyName = "ModifiedDate";

        //todo: commands only in context and create context using ActivatorUtilities.CreateInstance

        public MergeDataSyncer(
            IEnumerable<IContentItemDataSyncer> itemSyncers,
            IDataSyncPartDataSyncer dataSyncPartDataSyncer,
            ISyncNameProvider syncNameProvider,
            IMergeNodeCommand mergeNodeCommand,
            IReplaceRelationshipsCommand replaceRelationshipsCommand,
            IContentItemVersionFactory contentItemVersionFactory,
            IPublishedContentItemVersion publishedContentItemVersion,
            IPreviewContentItemVersion previewContentItemVersion,
            IServiceProvider serviceProvider,
            IDataSyncCluster dataSyncCluster,
            IContentItemsService contentItemsService,
            ILogger<MergeDataSyncer> logger)
        {
            _itemSyncers = itemSyncers.OrderByDescending(s => s.Priority);
            _dataSyncPartDataSyncer = dataSyncPartDataSyncer;
            _syncNameProvider = syncNameProvider;
            MergeNodeCommand = mergeNodeCommand;
            _replaceRelationshipsCommand = replaceRelationshipsCommand;
            _contentItemVersionFactory = contentItemVersionFactory;
            _publishedContentItemVersion = publishedContentItemVersion;
            _previewContentItemVersion = previewContentItemVersion;
            _serviceProvider = serviceProvider;
            _dataSyncCluster = dataSyncCluster;
            _contentItemsService = contentItemsService;
            _logger = logger;

            _dataSyncMergeContext = null;
            _incomingPreviewContentPickerRelationships = null;
        }

        public async Task<IAllowSync> SyncToDataSyncReplicaSetIfAllowed(
            IDataSyncReplicaSet dataSyncReplicaSet,
            ContentItem contentItem,
            IContentManager contentManager,
            IDataMergeContext? parentDataSyncMergeContext = null)
        {
            IAllowSync allowSync = await SyncAllowed(dataSyncReplicaSet, contentItem, contentManager, parentDataSyncMergeContext);

            if (allowSync.Result == AllowSyncResult.Allowed)
                await SyncToDataSyncReplicaSet();

            return allowSync;
        }

        public async Task<IAllowSync> SyncAllowed(
            IDataSyncReplicaSet dataSyncReplicaSet,
            ContentItem contentItem,
            IContentManager contentManager,
            IDataMergeContext? parentDataSyncMergeContext = null)
        {
            _logger.LogDebug("SyncAllowed to {DataSyncReplicaSetName} for '{ContentItem}' {ContentType}?",
                dataSyncReplicaSet.Name, contentItem.ToString(), contentItem.ContentType);

            _logger.LogDebug("ContentItem content: {Content}", ((JObject)contentItem.Content).ToString());

            // we use the existence of a GraphSync content part as a marker to indicate that the content item should be synced
            JObject? dataSyncPartContent = (JObject?)contentItem.Content[nameof(GraphSyncPart)];
            if (dataSyncPartContent == null)
                return AllowSync.NotRequired;

            //todo: ContentType belongs in the context, either combine helper & context, or supply context to helper?
            _syncNameProvider.ContentType = contentItem.ContentType;

            _dataSyncMergeContext = new DataMergeContext(
                this, _syncNameProvider, dataSyncReplicaSet, MergeNodeCommand, _replaceRelationshipsCommand,
                contentItem, contentManager, _contentItemVersionFactory, parentDataSyncMergeContext, _serviceProvider);

            await PopulateMergeNodeCommand(dataSyncPartContent);

            SetSourceNodeInReplaceRelationshipsCommand();

            //should it go in the context?
            _incomingPreviewContentPickerRelationships = await GetIncomingPreviewContentPickerRelationshipsWhenPublishing(
                dataSyncReplicaSet,
                dataSyncPartContent,
                contentItem.ContentItemId);

            return await SyncAllowed();
        }

        private async Task PopulateMergeNodeCommand(JObject dataSyncPartContent)
        {
            MergeNodeCommand.NodeLabels.UnionWith(await _syncNameProvider.NodeLabels());
            MergeNodeCommand.IdPropertyName = _syncNameProvider.IdPropertyName();

            //todo: we could move population of the time properties to later when syncing, rather than at syncallowed time

            // add created and modified dates to all content items
            if (_dataSyncMergeContext!.ContentItem.CreatedUtc.HasValue)
            {
                MergeNodeCommand.Properties.Add(await _syncNameProvider.PropertyName(CreatedDatePropertyName),
                    _dataSyncMergeContext.ContentItem.CreatedUtc.Value);
            }

            if (_dataSyncMergeContext.ContentItem.ModifiedUtc.HasValue)
            {
                MergeNodeCommand.Properties.Add(await _syncNameProvider.PropertyName(ModifiedDatePropertyName),
                    _dataSyncMergeContext.ContentItem.ModifiedUtc.Value);
            }

            await _dataSyncPartDataSyncer.AddSyncComponents(dataSyncPartContent, _dataSyncMergeContext!);
        }

        private async Task<IAllowSync> SyncAllowed()
        {
            IAllowSync syncAllowed = new AllowSync();

            foreach (IContentItemDataSyncer itemSyncer in _itemSyncers)
            {
                //todo: allow syncers to chain or not? probably not
                if (itemSyncer.CanSync(_dataSyncMergeContext!.ContentItem))
                {
                    await itemSyncer.AllowSync(_dataSyncMergeContext, syncAllowed);
                }
            }

            return syncAllowed;
        }

        public async Task<IMergeNodeCommand?> SyncToDataSyncReplicaSet()
        {
            _logger.LogDebug("Syncing {ContentItem}.", _dataSyncMergeContext?.ContentItem.ToString());

            await SyncEmbedded();

            _logger.LogInformation($"Syncing {_dataSyncMergeContext!.ContentItem.ContentType} : {_dataSyncMergeContext.ContentItem.ContentItemId} to {MergeNodeCommand}");
            await SyncComponentsToDataSyncReplicaSet();

            return MergeNodeCommand;
        }

        public async Task<IMergeDataSyncer?> SyncEmbedded(ContentItem contentItem)
        {
            _logger.LogDebug("Syncing embedded {ContentItem}.", contentItem.ToString());

            JObject? dataSyncPartContent = (JObject?)contentItem.Content[nameof(GraphSyncPart)];
            if (dataSyncPartContent == null)
                return null;

            var embeddedMergeContext = _dataSyncMergeContext!.ChildContexts
                .Single(c => c.ContentItem.ContentItemId == contentItem.ContentItemId);

            _logger.LogDebug("Found existing DataMergeContext for {ContentItem}.", contentItem.ToString());

            var embeddedMergeDataSyncer = (MergeDataSyncer)embeddedMergeContext.MergeDataSyncer;

            if (!embeddedMergeDataSyncer._syncNameProvider.DataSyncPartSettings.PreexistingNode)
            {
                await ((MergeDataSyncer)embeddedMergeContext.MergeDataSyncer).SyncEmbedded();
            }

            return embeddedMergeDataSyncer;
        }

        private async Task SyncEmbedded()
        {
            if (_dataSyncMergeContext == null)
                throw new DataSyncException($"You must call {nameof(SyncAllowed)} first.");

            await AddContentPartSyncComponents();

            //todo: bit hacky. best way to do this? remove this now?
            // work-around new taxonomy terms created with only DisplayText set
            if (!MergeNodeCommand.Properties.ContainsKey(_syncNameProvider.IdPropertyName())
                && MergeNodeCommand.Properties.ContainsKey(TitlePartDataSyncer.NodeTitlePropertyName))
            {
                MergeNodeCommand.IdPropertyName = TitlePartDataSyncer.NodeTitlePropertyName;
            }

            _dataSyncMergeContext.ExtraCommands.AddRange(
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
            IDataSyncReplicaSet dataSyncReplicaSet,
            dynamic dataSyncPartContent,
            string contentItemId)
        {
            // we only need to recreate incoming relationships
            // if we're publishing and there isn't currently a published version
            if (dataSyncReplicaSet.Name != DataSyncReplicaSetNames.Published
                || await _contentItemsService.HasExistingPublishedVersion(contentItemId))
            {
                return Enumerable.Empty<INodeWithOutgoingRelationships>();
            }

            var blackListNodeLabels = new[] { "resource", "html", "htmlshared" };
            var nodeLabels = MergeNodeCommand.NodeLabels
                .Where(l => blackListNodeLabels.All(blnl => !blnl.Equals(l.ToLower()))).ToArray();
            if (!nodeLabels.Any())
            {
                return Enumerable.Empty<INodeWithOutgoingRelationships>();
            }

            // allow sync is called concurrently for preview and published
            // so we could get the before or after incoming relationships
            // either should do, but perhaps we should do it serially to consistently fetch the _before_ incoming relationships?
            IGetIncomingContentPickerRelationshipsQuery getDraftRelationshipsQuery =
                _serviceProvider.GetRequiredService<IGetIncomingContentPickerRelationshipsQuery>();

            getDraftRelationshipsQuery.NodeLabels = nodeLabels;
            getDraftRelationshipsQuery.IdPropertyName = MergeNodeCommand.IdPropertyName;
            getDraftRelationshipsQuery.IdPropertyValue = _syncNameProvider.GetNodeIdPropertyValue(
                dataSyncPartContent, _previewContentItemVersion);

            IEnumerable<INodeWithOutgoingRelationships?> incomingContentPickerRelationshipsOrDefault =
                await _dataSyncCluster.Run(_previewContentItemVersion.DataSyncReplicaSetName, getDraftRelationshipsQuery);

            #pragma warning disable S1905 // Sonar needs updating to know about nullable references
            return incomingContentPickerRelationshipsOrDefault
                    .Where(n => n != null)
                    .Cast<INodeWithOutgoingRelationships>();
            #pragma warning restore S1905
        }

        private async Task AddContentPartSyncComponents()
        {
            foreach (IContentItemDataSyncer itemSyncer in _itemSyncers)
            {
                //todo: allow syncers to chain or not?
                if (itemSyncer.CanSync(_dataSyncMergeContext!.ContentItem))
                {
                    await itemSyncer.AddSyncComponents(_dataSyncMergeContext);
                }
            }
        }

        //todo: should we add a AddIdSyncComponents method?
        private Task SyncComponentsToDataSyncReplicaSet()
        {
            var commands = MoreEnumerable
                .TraverseBreadthFirst((IDataMergeContext)_dataSyncMergeContext!, ctx => ctx!.ChildContexts)
                .SelectMany(ctx =>
                {
                    var nodeCommands = new List<ICommand>();

                    if (ctx.ReplaceRelationshipsCommand.Relationships.Any())
                        nodeCommands.Add(ctx.ReplaceRelationshipsCommand);

                    if (ctx.ExtraCommands.Any())
                        nodeCommands.AddRange(ctx.ExtraCommands);

                    if (!ctx.SyncNameProvider.DataSyncPartSettings.PreexistingNode)
                        nodeCommands.Add(ctx.MergeNodeCommand);

                    return nodeCommands;
                })
                .Reverse()
                .ToArray();

            return _dataSyncMergeContext!.DataSyncReplicaSet.Run(commands);
        }

        private void SetSourceNodeInReplaceRelationshipsCommand()
        {
            _replaceRelationshipsCommand.SourceNodeLabels = new HashSet<string>(MergeNodeCommand.NodeLabels);
            _replaceRelationshipsCommand.SourceIdPropertyName = MergeNodeCommand.IdPropertyName;
            //todo: helper for this, used elsewhere
            _replaceRelationshipsCommand.SourceIdPropertyValue =
                _dataSyncMergeContext!.MergeNodeCommand.Properties[_dataSyncMergeContext.MergeNodeCommand.IdPropertyName!];
        }
    }
}
