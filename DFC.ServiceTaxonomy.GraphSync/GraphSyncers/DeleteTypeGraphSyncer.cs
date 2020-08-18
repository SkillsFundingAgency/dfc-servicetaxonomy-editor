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
        private readonly IGraphSyncHelper _graphSyncHelper;
        private readonly ISession _session;
        private readonly ILogger<DeleteTypeGraphSyncer> _logger;

        public DeleteTypeGraphSyncer(
            IGraphCluster graphCluster,
            IDeleteNodesByTypeCommand deleteNodesByTypeCommand,
            IGraphSyncHelper graphSyncHelper,
            ISession session,
            ILogger<DeleteTypeGraphSyncer> logger)
        {
            _graphCluster = graphCluster;
            _deleteNodesByTypeCommand = deleteNodesByTypeCommand;
            _graphSyncHelper = graphSyncHelper;
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
