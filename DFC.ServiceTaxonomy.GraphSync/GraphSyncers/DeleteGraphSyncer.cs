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
using OrchardCore.ContentManagement;
using YesSql;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    public class DeleteGraphSyncer : IDeleteGraphSyncer
    {
        private readonly IEnumerable<IContentItemGraphSyncer> _itemSyncers;
        private readonly IGraphCluster _graphCluster;
        private readonly IDeleteNodeCommand _deleteNodeCommand;
        private readonly IGraphSyncHelper _graphSyncHelper;
        private readonly IContentManager _contentManager;
        private readonly IDeleteNodesByTypeCommand _deleteNodesByTypeCommand;
        private readonly ISession _session;
        private readonly ILogger<DeleteGraphSyncer> _logger;
        private GraphDeleteItemSyncContext? _graphDeleteItemSyncContext;

        public DeleteGraphSyncer(
            IEnumerable<IContentItemGraphSyncer> itemSyncers,
            IGraphCluster graphCluster,
            IDeleteNodesByTypeCommand deleteNodesByTypeCommand,
            IDeleteNodeCommand deleteNodeCommand,
            IGraphSyncHelper graphSyncHelper,
            IContentManager contentManager,
            ISession session,
            ILogger<DeleteGraphSyncer> logger)
        {
            _itemSyncers = itemSyncers.OrderByDescending(s => s.Priority);
            _graphCluster = graphCluster;
            _deleteNodeCommand = deleteNodeCommand;
            _graphSyncHelper = graphSyncHelper;
            _contentManager = contentManager;
            _deleteNodesByTypeCommand = deleteNodesByTypeCommand;
            _session = session;
            _logger = logger;

            _graphDeleteItemSyncContext = null;
        }

        public async Task DeleteNodesByType(string graphReplicaSetName, string contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType))
                return;

            _logger.LogInformation($"Sync: deleting all nodes of {contentType}");

            _graphSyncHelper.ContentType = contentType;

            _deleteNodesByTypeCommand.NodeLabels.UnionWith(await _graphSyncHelper.NodeLabels(contentType));

            try
            {
                await _graphCluster.Run(graphReplicaSetName, _deleteNodesByTypeCommand);
            }
            //TODO : specify which exceptions to handle?
            //todo: move this into task?
            catch
            {
                //this forces a rollback of the current OC db transaction
                _session.Cancel();
                throw;
            }
        }

        public async Task Delete()
        {
            await Delete("deleting");
        }

        public async Task Unpublish()
        {
            //todo: ContentPickerRelationshipProperties for delete too?
            await Delete("unpublishing", ContentPickerFieldGraphSyncer.ContentPickerRelationshipProperties);
        }

        //todo: needs to be called from embedded for taxonomy
        public Task<IAllowSyncResult> DeleteAllowed(
            ContentItem contentItem,
            IContentItemVersion contentItemVersion,
            //string operation,
            IEnumerable<KeyValuePair<string, object>>? deleteIncomingRelationshipsProperties = null, // put into context
            IGraphDeleteItemSyncContext? parentContext = null)
        {
            _graphSyncHelper.ContentType = contentItem.ContentType;

            if (contentItem.Content.GraphSyncPart == null || _graphSyncHelper.GraphSyncPartSettings.PreexistingNode)
                return Task.FromResult(AllowSyncResult.NotRequired);

            _graphDeleteItemSyncContext = new GraphDeleteItemSyncContext(
                contentItem,
                _graphSyncHelper,
                _contentManager,
                contentItemVersion,
                parentContext);

            return Task.FromResult((IAllowSyncResult)new AllowSyncResult());
        }

        private async Task Delete(
            //ContentItem contentItem,
            //IContentItemVersion contentItemVersion,
            string operation,
            IEnumerable<KeyValuePair<string, object>>? deleteIncomingRelationshipsProperties = null)
        {
            if (_graphDeleteItemSyncContext == null)
                throw new GraphSyncException($"You must call {nameof(DeleteAllowed)} before calling {nameof(Delete)}.");

            _logger.LogInformation($"Sync: {operation} '{_graphDeleteItemSyncContext.ContentItem.DisplayText}' {_graphDeleteItemSyncContext.ContentItem.ContentType} ({_graphDeleteItemSyncContext.ContentItem.ContentItemId}) from {_graphDeleteItemSyncContext.ContentItemVersion.GraphReplicaSetName} replica set.");

            //todo: move into method
            _deleteNodeCommand.NodeLabels = new HashSet<string>(await _graphSyncHelper.NodeLabels());
            _deleteNodeCommand.IdPropertyName = _graphSyncHelper.IdPropertyName();
            _deleteNodeCommand.IdPropertyValue =
                _graphSyncHelper.GetIdPropertyValue(_graphDeleteItemSyncContext.ContentItem.Content.GraphSyncPart, _graphDeleteItemSyncContext.ContentItemVersion);
            _deleteNodeCommand.DeleteNode = !_graphSyncHelper.GraphSyncPartSettings.PreexistingNode;
            _deleteNodeCommand.DeleteIncomingRelationshipsProperties = deleteIncomingRelationshipsProperties;

            await ContentPartDelete();

            await DeleteFromGraphReplicaSet(_graphDeleteItemSyncContext.ContentItemVersion);
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

        //todo: 2x delete node commands
        //todo: not deleted

        private async Task DeleteFromGraphReplicaSet(IContentItemVersion contentItemVersion)
            //IGraphReplicaSet graphReplicaSet)
        {
            List<ICommand> commands = new List<ICommand> {_deleteNodeCommand};

            if (_graphDeleteItemSyncContext!.Commands.Any())
                commands.AddRange(_graphDeleteItemSyncContext.Commands);

            await _graphCluster.Run(contentItemVersion.GraphReplicaSetName, commands.ToArray());
            //await graphReplicaSet.Run(commands.ToArray());
        }
    }
}
