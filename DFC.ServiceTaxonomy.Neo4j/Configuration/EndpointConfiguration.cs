namespace DFC.ServiceTaxonomy.Neo4j.Configuration
{
    public class EndpointConfiguration
    {
        public string? Uri { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public bool Enabled { get; set; }
        public bool Primary { get; set; }
    }
}
