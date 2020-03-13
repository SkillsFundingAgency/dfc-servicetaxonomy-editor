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
        private readonly IDeleteNodesByTypeCommand _deleteNodesByTypeCommand;
        private readonly IGraphSyncPartIdProperty _graphSyncPartIdProperty;
        private readonly ISession _session;
        private readonly ILogger<DeleteGraphSyncer> _logger;

        //todo: have as setting of activity, or graph sync content part settings
        private const string NcsPrefix = "ncs__";

        public DeleteGraphSyncer(
            IGraphDatabase graphDatabase,
            IDeleteNodesByTypeCommand deleteNodesByTypeCommand,
            IDeleteNodeCommand deleteNodeCommand,
            IGraphSyncPartIdProperty graphSyncPartIdProperty,
            ISession session,
            ILogger<DeleteGraphSyncer> logger)
        {
            _graphDatabase = graphDatabase;
            _deleteNodeCommand = deleteNodeCommand;
            _deleteNodesByTypeCommand = deleteNodesByTypeCommand;
            _graphSyncPartIdProperty = graphSyncPartIdProperty;
            _session = session;
            _logger = logger;
        }

        public async Task DeleteNodesByType(string nodeType)
        {
            if (string.IsNullOrWhiteSpace(nodeType))
                return;

            _deleteNodesByTypeCommand.NodeLabels = new HashSet<string> { NcsPrefix + nodeType };

            try
            {
                await _graphDatabase.Run(_deleteNodesByTypeCommand);
            }
            //TODO : specify which exceptions to handle?
            catch
            {
                //this forces a rollback of the currect OC db transaction
                _session.Cancel();
                throw;
            }

            _logger.LogInformation($"Sync: deleting all nodes of {nodeType}");
        }

        public async Task DeleteFromGraph(ContentItem contentItem)
        {
            if (contentItem.Content.GraphSyncPart == null)
                return;

            _logger.LogInformation($"Sync: deleting {contentItem.ContentType}");

            _deleteNodeCommand.NodeLabels = new HashSet<string> { NcsPrefix + contentItem.ContentType };
            _deleteNodeCommand.IdPropertyName = _graphSyncPartIdProperty.Name;
            _deleteNodeCommand.IdPropertyValue = _graphSyncPartIdProperty.Value(contentItem.Content.GraphSyncPart);

            //todo: if preexistingnode set, just delete the relationships, not the node itself

            try
            {
                await _graphDatabase.Run(_deleteNodeCommand);
            }
            //TODO : specify which exceptions to handle?
            catch
            {
                //this forces a rollback of the currect OC db transaction
                _session.Cancel();
                throw;
            }
        }
    }
}
