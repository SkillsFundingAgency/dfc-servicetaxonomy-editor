using System;
using System.Collections.Generic;
using System.Linq;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Results.ValidateAndRepair
{
    public interface IValidateAndRepairResults
    {
        bool Cancelled { get; }
        DateTime LastSync { get; }
        List<ValidateAndRepairResult> GraphInstanceResults { get; }
        public bool AnyRepairFailures { get; }
    }

    public class ValidationAlreadyInProgressResult : IValidateAndRepairResults
    {
        public static readonly IValidateAndRepairResults Instance = new ValidationAlreadyInProgressResult();

        public bool Cancelled => true;
        public DateTime LastSync => throw new NotImplementedException();
        public List<ValidateAndRepairResult> GraphInstanceResults => throw new NotImplementedException();
        public bool AnyRepairFailures => throw new NotImplementedException();
    }

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
