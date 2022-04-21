using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Contexts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Exceptions;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Fields;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Items;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Results.AllowSync;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Results.AllowSync;
using DFC.ServiceTaxonomy.DataSync.Interfaces;
using DFC.ServiceTaxonomy.DataSync.Services;
using Microsoft.Extensions.Logging;
using MoreLinq;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers
{
    public class DeleteDataSyncer : IDeleteDataSyncer
    {
        private readonly IEnumerable<IContentItemDataSyncer> _itemSyncers;
        private readonly IDataSyncCluster _dataSyncCluster;
        private readonly IDeleteNodeCommand _deleteNodeCommand;
        private readonly ISyncNameProvider _syncNameProvider;
        private readonly IContentManager _contentManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DeleteDataSyncer> _logger;
        private DataDeleteContext? _dataSyncDeleteItemSyncContext;

        public DeleteDataSyncer(
            IEnumerable<IContentItemDataSyncer> itemSyncers,
            IDataSyncCluster dataSyncCluster,
            IDeleteNodeCommand deleteNodeCommand,
            ISyncNameProvider syncNameProvider,
            IContentManager contentManager,
            IServiceProvider serviceProvider,
            ILogger<DeleteDataSyncer> logger)
        {
            _itemSyncers = itemSyncers.OrderByDescending(s => s.Priority);
            _dataSyncCluster = dataSyncCluster;
            _deleteNodeCommand = deleteNodeCommand;
            _syncNameProvider = syncNameProvider;
            _contentManager = contentManager;
            _serviceProvider = serviceProvider;
            _logger = logger;

            _dataSyncDeleteItemSyncContext = null;
        }

        public string? DataSyncReplicaSetName => _dataSyncDeleteItemSyncContext?.ContentItemVersion.DataSyncReplicaSetName;

        public async Task<IAllowSync> DeleteAllowed(ContentItem contentItem,
            IContentItemVersion contentItemVersion,
            SyncOperation syncOperation,
            IEnumerable<KeyValuePair<string, object>>? deleteIncomingRelationshipsProperties = null,
            IDataDeleteContext? parentContext = null)
        {
            if (contentItem.Content.GraphSyncPart == null)
            {
                return AllowSync.NotRequired;
            }

            _syncNameProvider.ContentType = contentItem.ContentType;

            if (_syncNameProvider.DataSyncPartSettings.PreexistingNode)
            {
                return AllowSync.NotRequired;
            }

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
                    ContentPickerFieldDataSyncer.ContentPickerRelationshipProperties);
            }

            _dataSyncDeleteItemSyncContext = new DataDeleteContext(
                contentItem, _deleteNodeCommand, this, syncOperation, _syncNameProvider,
                _contentManager, contentItemVersion, allDeleteIncomingRelationshipsProperties, parentContext,
                _serviceProvider);

            return await DeleteAllowed();
        }

        private async Task<IAllowSync> DeleteAllowed()
        {
            IAllowSync syncAllowed = new AllowSync();

            foreach (IContentItemDataSyncer itemSyncer in _itemSyncers)
            {
                //todo: allow syncers to chain or not? probably not
                if (itemSyncer.CanSync(_dataSyncDeleteItemSyncContext!.ContentItem))
                {
                    await itemSyncer.AllowDelete(_dataSyncDeleteItemSyncContext, syncAllowed);
                }
            }

            return syncAllowed;
        }

        public async Task Delete()
        {
            await DeleteEmbedded();

            await DeleteFromDataSyncReplicaSet();
        }

        private async Task DeleteEmbedded()
        {
            if (_dataSyncDeleteItemSyncContext == null)
                throw new DataSyncException($"You must call {nameof(DeleteAllowed)} first.");

            _logger.LogInformation("{DeleteOperation} '{ContentItemDisplayText}' {ContentType} ({ContentItemId}) from {DataSyncReplicaSetName} replica set.",
            _dataSyncDeleteItemSyncContext.SyncOperation.ToString(),
            _dataSyncDeleteItemSyncContext.ContentItem.DisplayText,
            _dataSyncDeleteItemSyncContext.ContentItem.ContentType,
            _dataSyncDeleteItemSyncContext.ContentItem.ContentItemId,
            _dataSyncDeleteItemSyncContext.ContentItemVersion.DataSyncReplicaSetName);

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

            var embeddedDeleteContext = _dataSyncDeleteItemSyncContext!.ChildContexts
                .Single(c => c.ContentItem.ContentItemId == contentItem.ContentItemId);

            var embeddedDeleteDataSyncer = (DeleteDataSyncer)embeddedDeleteContext.DeleteDataSyncer;

            if (embeddedDeleteDataSyncer._syncNameProvider.DataSyncPartSettings.PreexistingNode)
                return;

            await ((DeleteDataSyncer)embeddedDeleteContext.DeleteDataSyncer).DeleteEmbedded();
        }

        private async Task PopulateDeleteNodeCommand()
        {
            _deleteNodeCommand.NodeLabels = new HashSet<string>(await _syncNameProvider.NodeLabels());
            _deleteNodeCommand.IdPropertyName = _syncNameProvider.IdPropertyName();
            _deleteNodeCommand.IdPropertyValue =
                _syncNameProvider.GetNodeIdPropertyValue(_dataSyncDeleteItemSyncContext!.ContentItem.Content.GraphSyncPart,
                    _dataSyncDeleteItemSyncContext.ContentItemVersion);
            _deleteNodeCommand.DeleteNode = !_syncNameProvider.DataSyncPartSettings.PreexistingNode;
            _deleteNodeCommand.DeleteIncomingRelationshipsProperties =
                _dataSyncDeleteItemSyncContext.DeleteIncomingRelationshipsProperties;
        }

        private async Task ContentPartDelete()
        {
            foreach (IContentItemDataSyncer itemSyncer in _itemSyncers)
            {
                //todo: allow syncers to chain or not? probably not
                //todo: code shared with MergeDataSyncer. might want to introduce common base if sharing continues
                if (itemSyncer.CanSync(_dataSyncDeleteItemSyncContext!.ContentItem))
                {
                    await itemSyncer.DeleteComponents(_dataSyncDeleteItemSyncContext);
                }
            }
        }

        private Task DeleteFromDataSyncReplicaSet()
        {
            var breadthFirstContexts = MoreEnumerable
                .TraverseBreadthFirst((IDataDeleteContext)_dataSyncDeleteItemSyncContext!, ctx => ctx!.ChildContexts)
                .Select(ctx => ctx.DeleteNodeCommand)
                .Cast<ICommand>()
                .ToArray();

            return _dataSyncCluster.Run(_dataSyncDeleteItemSyncContext!.ContentItemVersion.DataSyncReplicaSetName, breadthFirstContexts);
        }
    }
}
