using System;
using System.Collections.Generic;
using DFC.ServiceTaxonomy.DataSync.Enums;
using DFC.ServiceTaxonomy.DataSync.Interfaces;
using Microsoft.Extensions.Logging;

namespace DFC.ServiceTaxonomy.DataSync.Models
{
    internal class DataSyncReplicaSetLowLevel : DataSyncReplicaSet, IDataSyncReplicaSetLowLevel
    {
        internal DataSyncReplicaSetLowLevel(
            string name,
            IEnumerable<DataSync> graphInstances,
            ILogger logger,
            int? limitToGraphInstance = null)
            : base(name, graphInstances, logger, limitToGraphInstance)
        {
            foreach (var graph in _graphInstances)
            {
                graph.DataSyncReplicaSetLowLevel = this;
            }

            if (InstanceCount > 64)
                throw new ArgumentException("A max of 64 graph instances in the replica set is supported.");
        }

        public DataSync[] DataSyncInstances
        {
            get => _graphInstances;
        }

        public IDataSyncReplicaSetLowLevel CloneLimitedToGraphInstance(int instance)
        {
            return new DataSyncReplicaSetLowLevel(Name, _graphInstances, _logger, instance);
        }

        public DisabledStatus Disable(int instance)
        {
            throw new NotImplementedException("Replica disabling is not available.");
        }

        public EnabledStatus Enable(int instance)
        {
            throw new NotImplementedException("Replica disabling is not available.");
        }

        public string ToTraceString()
        {
            return "Replica disabling is not available.";
        }
    }
}
