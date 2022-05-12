namespace DFC.ServiceTaxonomy.GraphSync.CosmosDb
{
    public class CosmosDbOptionEndpoint
    {
        public string? ConnectionString { get; set; }

        public string? DatabaseName { get; set; }

        public string? ContainerName { get; set; }
    }
}
