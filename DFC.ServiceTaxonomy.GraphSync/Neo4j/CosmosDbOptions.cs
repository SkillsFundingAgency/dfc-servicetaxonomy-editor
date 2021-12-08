using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.GraphSync.CosmosDb
{
    public class CosmosDbOptions
    {
        public const string CosmosDb = "CosmosDb";

        public List<string> Endpoints { get; set; } = new List<string>();
    }
}
