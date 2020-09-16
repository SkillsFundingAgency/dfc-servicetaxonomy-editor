using System;
using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.ValidateAndRepair;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Results.ValidateAndRepair
{
    public class ValidateAndRepairResults : IValidateAndRepairResults
    {
        public bool Cancelled => false;
        public DateTime LastSync { get; }
        public List<ValidateAndRepairResult> GraphInstanceResults { get; }
        public bool AnyRepairFailures => GraphInstanceResults.SelectMany(r => r.RepairFailures).Any();

        public ValidateAndRepairResult AddNewValidateAndRepairResult(
            string graphReplicaSetName,
            int graphInstance,
            string endpointName,
            string graphName,
            bool defaultGraph)
        {
            ValidateAndRepairResult result = new ValidateAndRepairResult(
                graphReplicaSetName,
                graphInstance,
                endpointName,
                graphName,
                defaultGraph);
            GraphInstanceResults.Add(result);

            return result;
        }

        public ValidateAndRepairResults(DateTime lastSync)
        {
            LastSync = lastSync;
            GraphInstanceResults = new List<ValidateAndRepairResult>();
        }
    }
}
