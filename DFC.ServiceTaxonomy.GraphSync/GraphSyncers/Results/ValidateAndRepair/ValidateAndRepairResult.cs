using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Results.ValidateAndRepair
{
    public class ValidateAndRepairResult
    {
        public string GraphReplicaSetName { get; }
        public int GraphInstance { get; }
        public string EndpointName { get; }
        public string GraphName { get; }
        public bool DefaultGraph { get; }

        public List<ValidatedContentItem> Validated { get; } = new List<ValidatedContentItem>();
        public List<ValidationFailure> ValidationFailures { get; } = new List<ValidationFailure>();
        public List<ValidatedContentItem> Repaired { get; } = new List<ValidatedContentItem>();
        public List<RepairFailure> RepairFailures { get; } = new List<RepairFailure>();

        public ValidateAndRepairResult(
            string graphReplicaSetName,
            int graphInstance,
            string endpointName,
            string graphName,
            bool defaultGraph)
        {
            GraphReplicaSetName = graphReplicaSetName;
            GraphInstance = graphInstance;
            EndpointName = endpointName;
            GraphName = graphName;
            DefaultGraph = defaultGraph;
        }
    }
}
