using OrchardCore.Queries;

namespace DFC.ServiceTaxonomy.Cypher.Models
{
    public class CypherQuery : Query
    {
        public string Template { get; set; }

        public string Parameters { get; set; }

        public string ResultModelType { get; set; }

        public bool ReturnDocuments { get; set; }

        public CypherQuery() : base("Cypher") { }
    }
}
