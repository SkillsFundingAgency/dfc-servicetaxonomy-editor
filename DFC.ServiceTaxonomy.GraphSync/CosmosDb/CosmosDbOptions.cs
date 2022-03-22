using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.GraphSync.CosmosDb
{
    public class CosmosDbOptions
    {
        public const string CosmosDb = "CosmosDb";

        public string ConnectionString {get; set;} = string.Empty;
        public string DatabaseName { get; set;} = string.Empty;
        public List<string> Endpoints { get; set; } = new List<string>();
    }
}
