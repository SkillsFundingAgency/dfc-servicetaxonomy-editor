namespace DFC.ServiceTaxonomy.Neo4j.Configuration
{
#pragma warning disable S101 // Types should be named in PascalCase
    public class Neo4jConfiguration
#pragma warning restore S101 // Types should be named in PascalCase
    {
        public EndpointConfiguration Endpoint { get; set; } = new EndpointConfiguration();
    }
}
