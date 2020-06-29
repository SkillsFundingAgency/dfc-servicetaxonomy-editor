using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.Neo4j.Configuration
{
#pragma warning disable S101 // Types should be named in PascalCase
    public class Neo4jConfiguration
#pragma warning restore S101 // Types should be named in PascalCase
    {
        public List<EndpointConfiguration> Endpoints { get; set; } = new List<EndpointConfiguration>();
        public List<GraphConfiguration> Graphs { get; set; } = new List<GraphConfiguration>();
    }
}
