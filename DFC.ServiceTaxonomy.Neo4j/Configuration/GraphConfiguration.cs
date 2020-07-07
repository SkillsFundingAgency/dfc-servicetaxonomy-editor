namespace DFC.ServiceTaxonomy.Neo4j.Configuration
{
    public class GraphConfiguration
    {
        public string? Endpoint { get; set; }
        public string? GraphName { get; set; }
        public bool DefaultGraph { get; set; }
        public bool Enabled { get; set; }
    }
}
