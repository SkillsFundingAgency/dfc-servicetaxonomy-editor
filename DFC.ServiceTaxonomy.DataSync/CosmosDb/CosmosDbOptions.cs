using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.DataSync.CosmosDb
{
    public class CosmosDbOptions
    {
        public const string CosmosDb = "CosmosDb";
        public Dictionary<string, CosmosDbOptionEndpoint>? Endpoints { get; } = new Dictionary<string, CosmosDbOptionEndpoint>();
    }
}
