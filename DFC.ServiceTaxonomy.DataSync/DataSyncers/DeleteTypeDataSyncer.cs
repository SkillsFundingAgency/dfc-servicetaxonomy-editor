using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.DataSync.Interfaces;
using Microsoft.Extensions.Logging;
using YesSql;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers
{
    public class DeleteTypeDataSyncer : IDeleteTypeDataSyncer
    {
        private readonly IDataSyncCluster _dataSyncCluster;
        private readonly IDeleteNodesByTypeCommand _deleteNodesByTypeCommand;
        private readonly ISyncNameProvider _syncNameProvider;
        private readonly ISession _session;
        private readonly ILogger<DeleteTypeDataSyncer> _logger;

        public DeleteTypeDataSyncer(
            IDataSyncCluster dataSyncCluster,
            IDeleteNodesByTypeCommand deleteNodesByTypeCommand,
            ISyncNameProvider syncNameProvider,
            ISession session,
            ILogger<DeleteTypeDataSyncer> logger)
        {
            _dataSyncCluster = dataSyncCluster;
            _deleteNodesByTypeCommand = deleteNodesByTypeCommand;
            _syncNameProvider = syncNameProvider;
            _session = session;
            _logger = logger;
        }

        public async Task DeleteNodesByType(string dataSyncReplicaSetName, string contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType))
                return;

            _logger.LogInformation("Sync: deleting all nodes of {ContentType}", contentType);

            _syncNameProvider.ContentType = contentType;

            _deleteNodesByTypeCommand.NodeLabels.UnionWith(await _syncNameProvider.NodeLabels(contentType));

            try
            {
                await _dataSyncCluster.Run(dataSyncReplicaSetName, _deleteNodesByTypeCommand);
            }
            //todo: specify which exceptions to handle?
            catch
            {
                // this forces a rollback of the current OC db transaction
                await _session.CancelAsync();
                throw;
            }
        }
    }
}
