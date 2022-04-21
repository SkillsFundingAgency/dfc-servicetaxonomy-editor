using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.Interfaces;
using Microsoft.Extensions.Logging;

namespace DFC.ServiceTaxonomy.DataSync.Models
{
    public class DataSync : IDataSync
    {
        private readonly ILogger _logger;

        public string DataSyncName { get; }

        public bool DefaultDataSync { get; }

        public int Instance { get; }

        public IDataSyncReplicaSetLowLevel DataSyncReplicaSetLowLevel { get; internal set; }

        public IEndpoint Endpoint { get; }

        public DataSync(IEndpoint endpoint, string graphName, bool defaultGraph, int instance, ILogger logger)
        {
            _logger = logger;
            Endpoint = endpoint;
            DataSyncName = graphName;
            DefaultDataSync = defaultGraph;
            Instance = instance;

            // DataSyncReplicaSet will set this as part of the build process, before any consumer gets an instance of this class
            DataSyncReplicaSetLowLevel = default!;
        }

        public virtual Task<List<T>> Run<T>(params IQuery<T>[] queries)
        {
            ulong newInFlightCount = 0;

            _logger.LogTrace("Running batch of {NumberOfQueries} queries on {DataSyncName}, instance #{Instance}. Now {InFlightCount} queries/commands in flight.",
                queries.Length, DataSyncName, Instance, newInFlightCount);

            return Endpoint.Run(queries, DataSyncName, DefaultDataSync);
        }

        public virtual Task Run(params ICommand[] commands)
        {
            ulong newInFlightCount = 0;

            _logger.LogTrace("Running batch of {NumberOfQueries} commands on {DataSyncName}, instance #{Instance}. Now {InFlightCount} queries/commands in flight.",
                commands.Length, DataSyncName, Instance, newInFlightCount);

            return Endpoint.Run(commands, DataSyncName, DefaultDataSync);
        }

        public IDataSyncReplicaSetLowLevel GetReplicaSetLimitedToThisDataSync()
        {
            return DataSyncReplicaSetLowLevel.CloneLimitedToGraphInstance(Instance);
        }
    }
}
