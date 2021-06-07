using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Exceptions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Items;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.AllowSync;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Results.AllowSync;
using DFC.ServiceTaxonomy.GraphSync.Services;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using Microsoft.Extensions.Logging;
using MoreLinq;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    public class DeleteGraphSyncer : IDeleteGraphSyncer
    {
        private readonly IEnumerable<IContentItemGraphSyncer> _itemSyncers;
        private readonly IGraphCluster _graphCluster;
        private readonly IDeleteNodeCommand _deleteNodeCommand;
        private readonly ISyncNameProvider _syncNameProvider;
        private readonly IContentManager _contentManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DeleteGraphSyncer> _logger;
        private GraphDeleteContext? _graphDeleteItemSyncContext;

        public DeleteGraphSyncer(
            IEnumerable<IContentItemGraphSyncer> itemSyncers,
            IGraphCluster graphCluster,
            IDeleteNodeCommand deleteNodeCommand,
            ISyncNameProvider syncNameProvider,
            IContentManager contentManager,
            IServiceProvider serviceProvider,
            ILogger<DeleteGraphSyncer> logger)
        {
            _itemSyncers = itemSyncers.OrderByDescending(s => s.Priority);
            _graphCluster = graphCluster;
            _deleteNodeCommand = deleteNodeCommand;
            _syncNameProvider = syncNameProvider;
            _contentManager = contentManager;
            _serviceProvider = serviceProvider;
            _logger = logger;

            _graphDeleteItemSyncContext = null;
        }

        public string? GraphReplicaSetName => _graphDeleteItemSyncContext?.ContentItemVersion.GraphReplicaSetName;

        public async Task<IAllowSync> DeleteAllowed(ContentItem contentItem,
            IContentItemVersion contentItemVersion,
            SyncOperation syncOperation,
            IEnumerable<KeyValuePair<string, object>>? deleteIncomingRelationshipsProperties = null,
            IGraphDeleteContext? parentContext = null)
        {
            _syncNameProvider.ContentType = contentItem.ContentType;

            if (contentItem.Content.GraphSyncPart == null || _syncNameProvider.GraphSyncPartSettings.PreexistingNode)
                return AllowSync.NotRequired;

            //todo: helper for this
            var allDeleteIncomingRelationshipsProperties = new HashSet<KeyValuePair<string, object>>();

            if (deleteIncomingRelationshipsProperties != null)
            {
                allDeleteIncomingRelationshipsProperties.UnionWith(deleteIncomingRelationshipsProperties);
            }
            //todo: unions unnecessarily when called embeddedly : new deleteembeddedallowed method?
            if (syncOperation == SyncOperation.Unpublish)
            {
                allDeleteIncomingRelationshipsProperties.UnionWith(
                    ContentPickerFieldGraphSyncer.ContentPickerRelationshipProperties);
            }

            _graphDeleteItemSyncContext = new GraphDeleteContext(
                contentItem, _deleteNodeCommand, this, syncOperation, _syncNameProvider,
                _contentManager, contentItemVersion, allDeleteIncomingRelationshipsProperties, parentContext,
                _serviceProvider);

            return await DeleteAllowed();
        }

        private async Task<IAllowSync> DeleteAllowed()
        {
            IAllowSync syncAllowed = new AllowSync();

            foreach (IContentItemGraphSyncer itemSyncer in _itemSyncers)
            {
                //todo: allow syncers to chain or not? probably not
                if (itemSyncer.CanSync(_graphDeleteItemSyncContext!.ContentItem))
                {
                    await itemSyncer.AllowDelete(_graphDeleteItemSyncContext, syncAllowed);
                }
            }

            return syncAllowed;
        }

        public async Task Delete()
        {
            await DeleteEmbedded();

            await DeleteFromGraphReplicaSet();
        }

        private async Task DeleteEmbedded()
        {
            if (_graphDeleteItemSyncContext == null)
                throw new GraphSyncException($"You must call {nameof(DeleteAllowed)} first.");

            _logger.LogInformation("{DeleteOperation} '{ContentItemDisplayText}' {ContentType} ({ContentItemId}) from {GraphReplicaSetName} replica set.",
            _graphDeleteItemSyncContext.SyncOperation.ToString(),
            _graphDeleteItemSyncContext.ContentItem.DisplayText,
            _graphDeleteItemSyncContext.ContentItem.ContentType,
            _graphDeleteItemSyncContext.ContentItem.ContentItemId,
            _graphDeleteItemSyncContext.ContentItemVersion.GraphReplicaSetName);

            await PopulateDeleteNodeCommand();

            await ContentPartDelete();
        }

        public async Task<IAllowSync> DeleteIfAllowed(
            ContentItem contentItem,
            IContentItemVersion contentItemVersion,
            SyncOperation syncOperation,
            IEnumerable<KeyValuePair<string, object>>? deleteIncomingRelationshipsProperties = null)
        {
            IAllowSync allowSync = await DeleteAllowed(
                contentItem,
                contentItemVersion,
                syncOperation,
                deleteIncomingRelationshipsProperties);

            if (allowSync.Result == AllowSyncResult.Allowed)
                await Delete();

            return allowSync;
        }

        public async Task DeleteEmbedded(ContentItem contentItem)
        {
            if (contentItem.Content.GraphSyncPart == null)
                return;

            var embeddedDeleteContext = _graphDeleteItemSyncContext!.ChildContexts
                .Single(c => c.ContentItem.ContentItemId == contentItem.ContentItemId);

            var embeddedDeleteGraphSyncer = (DeleteGraphSyncer)embeddedDeleteContext.DeleteGraphSyncer;

            if (embeddedDeleteGraphSyncer._syncNameProvider.GraphSyncPartSettings.PreexistingNode)
                return;

            await ((DeleteGraphSyncer)embeddedDeleteContext.DeleteGraphSyncer).DeleteEmbedded();
        }

        private async Task PopulateDeleteNodeCommand()
        {
            _deleteNodeCommand.NodeLabels = new HashSet<string>(await _syncNameProvider.NodeLabels());
            _deleteNodeCommand.IdPropertyName = _syncNameProvider.IdPropertyName();
            _deleteNodeCommand.IdPropertyValue =
                _syncNameProvider.GetNodeIdPropertyValue(_graphDeleteItemSyncContext!.ContentItem.Content.GraphSyncPart,
                    _graphDeleteItemSyncContext.ContentItemVersion);
            _deleteNodeCommand.DeleteNode = !_syncNameProvider.GraphSyncPartSettings.PreexistingNode;
            _deleteNodeCommand.DeleteIncomingRelationshipsProperties =
                _graphDeleteItemSyncContext.DeleteIncomingRelationshipsProperties;
        }

        private async Task ContentPartDelete()
        {
            foreach (IContentItemGraphSyncer itemSyncer in _itemSyncers)
            {
                //todo: allow syncers to chain or not? probably not
                //todo: code shared with MergeGraphSyncer. might want to introduce common base if sharing continues
                if (itemSyncer.CanSync(_graphDeleteItemSyncContext!.ContentItem))
                {
                    await itemSyncer.DeleteComponents(_graphDeleteItemSyncContext);
                }
            }
        }

        private Task DeleteFromGraphReplicaSet()
        {
            var breadthFirstContexts = MoreEnumerable
                .TraverseBreadthFirst((IGraphDeleteContext)_graphDeleteItemSyncContext!, ctx => ctx!.ChildContexts)
                .Select(ctx => ctx.DeleteNodeCommand)
                .Cast<ICommand>()
                .ToArray();

            return _graphCluster.Run(_graphDeleteItemSyncContext!.ContentItemVersion.GraphReplicaSetName, breadthFirstContexts);
        }
    }
}
