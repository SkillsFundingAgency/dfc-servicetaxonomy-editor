using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.Neo4j.Configuration
{
    public class ReplicaSetConfiguration
    {
        public string? ReplicaSetName { get; set; }
        public List<GraphConfiguration> GraphInstances { get; set; } = new List<GraphConfiguration>();
    }
}
