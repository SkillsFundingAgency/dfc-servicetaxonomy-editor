using System;
using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Results.ValidateAndRepair;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Results.ValidateAndRepair
{
    public class ValidateAndRepairResults : IValidateAndRepairResults
    {
        public bool Cancelled => false;
        public DateTime LastSync { get; }
        public List<ValidateAndRepairResult> DataSyncInstanceResults { get; }
        public bool AnyRepairFailures => DataSyncInstanceResults.SelectMany(r => r.RepairFailures).Any();

        public ValidateAndRepairResult AddNewValidateAndRepairResult(
            string dataSyncReplicaSetName,
            int dataSyncInstance,
            string endpointName,
            string dataSyncName,
            bool defaultDataSync)
        {
            ValidateAndRepairResult result = new ValidateAndRepairResult(
                dataSyncReplicaSetName,
                dataSyncInstance,
                endpointName,
                dataSyncName,
                defaultDataSync);
            DataSyncInstanceResults.Add(result);

            return result;
        }

        public ValidateAndRepairResults(DateTime lastSync)
        {
            LastSync = lastSync;
            DataSyncInstanceResults = new List<ValidateAndRepairResult>();
        }
    }
}
