using System.Collections.Generic;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Results.ValidateAndRepair
{
    public class ValidateAndRepairResult
    {
        public string GraphReplicaSetName { get; }
        public int GraphInstance { get; }
        public string EndpointName { get; }
        public string GraphName { get; }
        public bool DefaultGraph { get; }

        public List<ContentItem> Validated { get; } = new List<ContentItem>();
        public List<ValidationFailure> ValidationFailures { get; } = new List<ValidationFailure>();
        public List<ContentItem> Repaired { get; } = new List<ContentItem>();
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
