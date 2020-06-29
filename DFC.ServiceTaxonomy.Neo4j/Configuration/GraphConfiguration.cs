using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.Neo4j.Configuration
{
    public class GraphConfiguration
    {
        public string? Name { get; set; }
        public List<GraphInstanceConfiguration> Instances { get; set; } = new List<GraphInstanceConfiguration>();
    }
}
