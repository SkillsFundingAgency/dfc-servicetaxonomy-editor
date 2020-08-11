using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Fields;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Items;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using YesSql;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    public class DeleteGraphSyncer : IDeleteGraphSyncer
    {
        private readonly IContentItemGraphSyncer _contentItemGraphSyncer;
        private readonly IGraphCluster _graphCluster;
        private readonly IDeleteNodeCommand _deleteNodeCommand;
        private readonly IGraphSyncHelper _graphSyncHelper;
        private readonly IDeleteNodesByTypeCommand _deleteNodesByTypeCommand;
        private readonly ISession _session;
        private readonly ILogger<DeleteGraphSyncer> _logger;

        public DeleteGraphSyncer(
            IContentItemGraphSyncer contentItemGraphSyncer,
            IGraphCluster graphCluster,
            IDeleteNodesByTypeCommand deleteNodesByTypeCommand,
            IDeleteNodeCommand deleteNodeCommand,
            IGraphSyncHelper graphSyncHelper,
            ISession session,
            ILogger<DeleteGraphSyncer> logger)
        {
            _contentItemGraphSyncer = contentItemGraphSyncer;
            _graphCluster = graphCluster;
            _deleteNodeCommand = deleteNodeCommand;
            _graphSyncHelper = graphSyncHelper;
            _deleteNodesByTypeCommand = deleteNodesByTypeCommand;
            _session = session;
            _logger = logger;
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

        public async Task Delete(ContentItem contentItem, IContentItemVersion contentItemVersion)
        {
            await Delete(contentItem, contentItemVersion, "deleting");
        }

        public async Task Unpublish(ContentItem contentItem, IContentItemVersion contentItemVersion)
        {
            await Delete(contentItem, contentItemVersion, "unpublishing", ContentPickerFieldGraphSyncer.ContentPickerRelationshipProperties);
        }

        private async Task Delete(
            ContentItem contentItem,
            IContentItemVersion contentItemVersion,
            string operation,
            IEnumerable<KeyValuePair<string, object>>? deleteIncomingRelationshipsProperties = null)
        {
            _graphSyncHelper.ContentType = contentItem.ContentType;

            if (contentItem.Content.GraphSyncPart == null || _graphSyncHelper.GraphSyncPartSettings.PreexistingNode)
                return;

            _logger.LogInformation($"Sync: {operation} '{contentItem.DisplayText}' {contentItem.ContentType} ({contentItem.ContentItemId}) from {contentItemVersion.GraphReplicaSetName} replica set.");

            _deleteNodeCommand.NodeLabels = new HashSet<string>(await _graphSyncHelper.NodeLabels());
            _deleteNodeCommand.IdPropertyName = _graphSyncHelper.IdPropertyName();
            _deleteNodeCommand.IdPropertyValue =
                _graphSyncHelper.GetIdPropertyValue(contentItem.Content.GraphSyncPart, contentItemVersion);
            _deleteNodeCommand.DeleteNode = !_graphSyncHelper.GraphSyncPartSettings.PreexistingNode;
            _deleteNodeCommand.DeleteIncomingRelationshipsProperties = deleteIncomingRelationshipsProperties;

            await _graphCluster.Run(contentItemVersion.GraphReplicaSetName, _deleteNodeCommand);
        }
    }
}
