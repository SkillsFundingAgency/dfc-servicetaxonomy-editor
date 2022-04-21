using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Results.ValidateAndRepair
{
    public class ValidateAndRepairResult
    {
        public string DataSyncReplicaSetName { get; }
        public int DataSyncInstance { get; }
        public string EndpointName { get; }
        public string DataSyncName { get; }
        public bool DefaultDataSync { get; }

        public List<ValidatedContentItem> Validated { get; } = new List<ValidatedContentItem>();
        public List<ValidationFailure> ValidationFailures { get; } = new List<ValidationFailure>();
        public List<ValidatedContentItem> Repaired { get; } = new List<ValidatedContentItem>();
        public List<RepairFailure> RepairFailures { get; } = new List<RepairFailure>();

        public ValidateAndRepairResult(
            string dataSyncReplicaSetName,
            int dataSyncInstance,
            string endpointName,
            string dataSyncName,
            bool defaultDataSync)
        {
            DataSyncReplicaSetName = dataSyncReplicaSetName;
            DataSyncInstance = dataSyncInstance;
            EndpointName = endpointName;
            DataSyncName = dataSyncName;
            DefaultDataSync = defaultDataSync;
        }
    }
}
