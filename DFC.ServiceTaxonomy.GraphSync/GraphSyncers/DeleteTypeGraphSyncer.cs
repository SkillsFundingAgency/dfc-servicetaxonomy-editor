using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using Microsoft.Extensions.Logging;
using YesSql;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    public class DeleteTypeGraphSyncer : IDeleteTypeGraphSyncer
    {
        private readonly IGraphCluster _graphCluster;
        private readonly IDeleteNodesByTypeCommand _deleteNodesByTypeCommand;
        private readonly ISyncNameProvider _syncNameProvider;
        private readonly ISession _session;
        private readonly ILogger<DeleteTypeGraphSyncer> _logger;

        public DeleteTypeGraphSyncer(
            IGraphCluster graphCluster,
            IDeleteNodesByTypeCommand deleteNodesByTypeCommand,
            ISyncNameProvider syncNameProvider,
            ISession session,
            ILogger<DeleteTypeGraphSyncer> logger)
        {
            _graphCluster = graphCluster;
            _deleteNodesByTypeCommand = deleteNodesByTypeCommand;
            _syncNameProvider = syncNameProvider;
            _session = session;
            _logger = logger;
        }

        public async Task DeleteNodesByType(string graphReplicaSetName, string contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType))
                return;

            _logger.LogInformation($"Sync: deleting all nodes of {contentType}");

            _syncNameProvider.ContentType = contentType;

            _deleteNodesByTypeCommand.NodeLabels.UnionWith(await _syncNameProvider.NodeLabels(contentType));

            try
            {
                await _graphCluster.Run(graphReplicaSetName, _deleteNodesByTypeCommand);
            }
            //todo: specify which exceptions to handle?
            catch
            {
                //this forces a rollback of the current OC db transaction
                _session.Cancel();
                throw;
            }
        }
    }
}
