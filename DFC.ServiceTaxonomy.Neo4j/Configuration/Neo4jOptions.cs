using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.Neo4j.Configuration
{
    //todo: rename Neo4jOptions
#pragma warning disable S101 // Types should be named in PascalCase
    public class Neo4jOptions
#pragma warning restore S101 // Types should be named in PascalCase
    {
        public const string Neo4j = "Neo4j";

        public List<EndpointConfiguration> Endpoints { get; set; } = new List<EndpointConfiguration>();
        public List<ReplicaSetConfiguration> ReplicaSets { get; set; } = new List<ReplicaSetConfiguration>();
    }
}
