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
        private readonly IGraphSyncHelper _graphSyncHelper;
        private readonly IContentManager _contentManager;
        private readonly ILogger<DeleteGraphSyncer> _logger;
        private GraphDeleteContext? _graphDeleteItemSyncContext;

        public DeleteGraphSyncer(
            IEnumerable<IContentItemGraphSyncer> itemSyncers,
            IGraphCluster graphCluster,
            IDeleteNodeCommand deleteNodeCommand,
            IGraphSyncHelper graphSyncHelper,
            IContentManager contentManager,
            ILogger<DeleteGraphSyncer> logger)
        {
            _itemSyncers = itemSyncers.OrderByDescending(s => s.Priority);
            _graphCluster = graphCluster;
            _deleteNodeCommand = deleteNodeCommand;
            _graphSyncHelper = graphSyncHelper;
            _contentManager = contentManager;
            _logger = logger;

            _graphDeleteItemSyncContext = null;
        }

        public async Task<IAllowSyncResult> DeleteAllowed(
            ContentItem contentItem,
            IContentItemVersion contentItemVersion,
            DeleteOperation deleteOperation,
            IEnumerable<KeyValuePair<string, object>>? deleteIncomingRelationshipsProperties = null,
            IGraphDeleteContext? parentContext = null)
        {
            _graphSyncHelper.ContentType = contentItem.ContentType;

            if (contentItem.Content.GraphSyncPart == null || _graphSyncHelper.GraphSyncPartSettings.PreexistingNode)
                return AllowSyncResult.NotRequired;

            //todo: helper for this
            var allDeleteIncomingRelationshipsProperties = new HashSet<KeyValuePair<string, object>>();

            if (deleteIncomingRelationshipsProperties != null)
            {
                allDeleteIncomingRelationshipsProperties.UnionWith(deleteIncomingRelationshipsProperties);
            }
            //todo: unions unnecessarily when called embeddedly : new deleteembeddedallowed method?
            if (deleteOperation == DeleteOperation.Unpublish)
            {
                allDeleteIncomingRelationshipsProperties.UnionWith(
                    ContentPickerFieldGraphSyncer.ContentPickerRelationshipProperties);
            }

            _graphDeleteItemSyncContext = new GraphDeleteContext(
                contentItem, _deleteNodeCommand, this, deleteOperation, _graphSyncHelper,
                _contentManager, contentItemVersion, allDeleteIncomingRelationshipsProperties, parentContext);

            parentContext?.AddChildContext(_graphDeleteItemSyncContext);

            return await DeleteAllowed();
        }

        private async Task<IAllowSyncResult> DeleteAllowed()
        {
            IAllowSyncResult syncAllowedResult = new AllowSyncResult();

            foreach (IContentItemGraphSyncer itemSyncer in _itemSyncers)
            {
                //todo: allow syncers to chain or not? probably not
                if (itemSyncer.CanSync(_graphDeleteItemSyncContext!.ContentItem))
                {
                    await itemSyncer.AllowDelete(_graphDeleteItemSyncContext, syncAllowedResult);
                }
            }

            return syncAllowedResult;
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

            _logger.LogInformation($"{(_graphDeleteItemSyncContext.DeleteOperation==DeleteOperation.Delete?"Deleting":"Unpublishing")} '{_graphDeleteItemSyncContext.ContentItem.DisplayText}' {_graphDeleteItemSyncContext.ContentItem.ContentType} ({_graphDeleteItemSyncContext.ContentItem.ContentItemId}) from {_graphDeleteItemSyncContext.ContentItemVersion.GraphReplicaSetName} replica set.");

            await PopulateDeleteNodeCommand();

            await ContentPartDelete();
        }

        public async Task<IAllowSyncResult> DeleteIfAllowed(
            ContentItem contentItem,
            IContentItemVersion contentItemVersion,
            DeleteOperation deleteOperation,
            IEnumerable<KeyValuePair<string, object>>? deleteIncomingRelationshipsProperties = null)
        {
            IAllowSyncResult allowSyncResult = await DeleteAllowed(
                contentItem,
                contentItemVersion,
                deleteOperation,
                deleteIncomingRelationshipsProperties);

            if (allowSyncResult.AllowSync == SyncStatus.Allowed)
                await Delete();

            return allowSyncResult;
        }

        public async Task DeleteEmbedded(ContentItem contentItem)
        {
            if (contentItem.Content.GraphSyncPart == null)
                return;

            var embeddedDeleteContext = _graphDeleteItemSyncContext!.ChildContexts
                .Single(c => c.ContentItem.ContentItemId == contentItem.ContentItemId);

            var embeddedDeleteGraphSyncer = (DeleteGraphSyncer)embeddedDeleteContext.DeleteGraphSyncer;

            if (embeddedDeleteGraphSyncer._graphSyncHelper.GraphSyncPartSettings.PreexistingNode)
                return;

            await ((DeleteGraphSyncer)embeddedDeleteContext.DeleteGraphSyncer).DeleteEmbedded();
        }

        private async Task PopulateDeleteNodeCommand()
        {
            _deleteNodeCommand.NodeLabels = new HashSet<string>(await _graphSyncHelper.NodeLabels());
            _deleteNodeCommand.IdPropertyName = _graphSyncHelper.IdPropertyName();
            _deleteNodeCommand.IdPropertyValue =
                _graphSyncHelper.GetIdPropertyValue(_graphDeleteItemSyncContext!.ContentItem.Content.GraphSyncPart,
                    _graphDeleteItemSyncContext.ContentItemVersion);
            _deleteNodeCommand.DeleteNode = !_graphSyncHelper.GraphSyncPartSettings.PreexistingNode;
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

        private async Task DeleteFromGraphReplicaSet()
        {
            var breadthFirstContexts = MoreEnumerable
                .TraverseBreadthFirst((IGraphDeleteContext)_graphDeleteItemSyncContext!, ctx => ctx!.ChildContexts)
                .Select(ctx => ctx.DeleteNodeCommand)
                .Cast<ICommand>()
                .ToArray();

            await _graphCluster.Run(_graphDeleteItemSyncContext!.ContentItemVersion.GraphReplicaSetName, breadthFirstContexts);
        }
    }
}
