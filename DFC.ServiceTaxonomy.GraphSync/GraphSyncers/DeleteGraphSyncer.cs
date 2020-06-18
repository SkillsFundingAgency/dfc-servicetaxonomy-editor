using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using YesSql;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    public class DeleteGraphSyncer : IDeleteGraphSyncer
    {
        private readonly IGraphDatabase _graphDatabase;
        private readonly IDeleteNodeCommand _deleteNodeCommand;
        private readonly IGraphSyncHelper _graphSyncHelper;
        private readonly IDeleteNodesByTypeCommand _deleteNodesByTypeCommand;
        private readonly ISession _session;
        private readonly ILogger<DeleteGraphSyncer> _logger;

        public DeleteGraphSyncer(
            IGraphDatabase graphDatabase,
            IDeleteNodesByTypeCommand deleteNodesByTypeCommand,
            IDeleteNodeCommand deleteNodeCommand,
            IGraphSyncHelper graphSyncHelper,
            ISession session,
            ILogger<DeleteGraphSyncer> logger)
        {
            _graphDatabase = graphDatabase;
            _deleteNodeCommand = deleteNodeCommand;
            _graphSyncHelper = graphSyncHelper;
            _deleteNodesByTypeCommand = deleteNodesByTypeCommand;
            _session = session;
            _logger = logger;
        }

        public async Task DeleteNodesByType(string contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType))
                return;

            _logger.LogInformation($"Sync: deleting all nodes of {contentType}");

            _graphSyncHelper.ContentType = contentType;

            _deleteNodesByTypeCommand.NodeLabels.UnionWith(await _graphSyncHelper.NodeLabels(contentType));

            try
            {
                await _graphDatabase.Run(_deleteNodesByTypeCommand);
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

        public async Task DeleteFromGraph(ContentItem contentItem)
        {

            _graphSyncHelper.ContentType = contentItem.ContentType;

            if (contentItem.Content.GraphSyncPart == null || _graphSyncHelper.GraphSyncPartSettings.PreexistingNode)
                return;

            _logger.LogInformation($"Sync: deleting {contentItem.ContentType}");

            _deleteNodeCommand.NodeLabels = new HashSet<string>(await _graphSyncHelper.NodeLabels());
            _deleteNodeCommand.IdPropertyName = _graphSyncHelper.IdPropertyName();
            _deleteNodeCommand.IdPropertyValue = _graphSyncHelper.GetIdPropertyValue(contentItem.Content.GraphSyncPart);
            _deleteNodeCommand.DeleteNode = !_graphSyncHelper.GraphSyncPartSettings.PreexistingNode;

            await _graphDatabase.Run(_deleteNodeCommand);
        }
    }
}
